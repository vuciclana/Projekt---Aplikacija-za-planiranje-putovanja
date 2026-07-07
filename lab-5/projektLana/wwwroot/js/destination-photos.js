(function () {
    const statusClasses = ['is-info', 'is-success', 'is-error'];
    const acceptedMimeTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];

    if (window.Dropzone) {
        window.Dropzone.autoDiscover = false;
    }

    function getAntiForgeryToken(section) {
        const form = section.closest('form');
        return form ? form.querySelector('input[name="__RequestVerificationToken"]')?.value : '';
    }

    function setStatus(section, message, type) {
        const status = section.querySelector('[data-photo-status]');
        status.textContent = message;
        statusClasses.forEach((className) => status.classList.remove(className));
        status.classList.add(`is-${type || 'info'}`);
    }

    function getClipboardImageFiles(items) {
        if (!items || !items.length) {
            return [];
        }

        return Array.from(items)
            .map((item) => item.kind === 'file' ? item.getAsFile() : null)
            .filter((file) => file && acceptedMimeTypes.includes(file.type));
    }

    function getUploadFormData(section, file) {
        const formData = new FormData();
        if (file) {
            formData.append('file', file);
        }

        formData.append('uploadSessionId', section.dataset.uploadSessionId || '');
        if (section.dataset.destinationId) {
            formData.append('id', section.dataset.destinationId);
        }

        return formData;
    }

    function formatBytes(bytes) {
        if (!bytes) return '0 B';
        const units = ['B', 'KB', 'MB', 'GB'];
        const index = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1);
        return `${(bytes / Math.pow(1024, index)).toFixed(index === 0 ? 0 : 1)} ${units[index]}`;
    }

    function renderPhotos(section, photos) {
        const list = section.querySelector('[data-photo-list]');
        const status = section.querySelector('[data-photo-status]');
        list.innerHTML = '';

        if (!photos.length) {
            setStatus(section, 'No photos uploaded yet.', 'info');
            return;
        }

        setStatus(section, `${photos.length} photo${photos.length === 1 ? '' : 's'} uploaded.`, 'success');
        photos.forEach((photo) => {
            const item = document.createElement('article');
            item.className = 'destination-photo-item';

            const image = document.createElement('img');
            image.src = photo.url;
            image.alt = photo.name;
            image.loading = 'lazy';

            const info = document.createElement('div');
            info.className = 'destination-photo-info';

            const name = document.createElement('strong');
            name.textContent = photo.name;

            const meta = document.createElement('span');
            meta.textContent = `${formatBytes(photo.size)} - ${photo.uploadedAt}`;

            const deleteButton = document.createElement('button');
            deleteButton.type = 'button';
            deleteButton.className = 'btn btn-sm btn-outline-danger destination-photo-delete';
            deleteButton.innerHTML = '<i class="bi bi-trash me-1"></i>Delete';
            deleteButton.addEventListener('click', async () => {
                deleteButton.disabled = true;
                const deleteUrl = `${section.dataset.deleteUrlBase}/${photo.id}?uploadSessionId=${encodeURIComponent(section.dataset.uploadSessionId)}`;
                const response = await fetch(deleteUrl, {
                    method: 'DELETE',
                    headers: {
                        'RequestVerificationToken': getAntiForgeryToken(section)
                    }
                });

                if (!response.ok) {
                    deleteButton.disabled = false;
                    setStatus(section, 'Could not delete the photo. Please try again.', 'error');
                    return;
                }

                setStatus(section, 'Photo deleted.', 'success');
                await loadPhotos(section);
            });

            info.append(name, meta);
            item.append(image, info, deleteButton);
            list.appendChild(item);
        });
    }

    async function loadPhotos(section) {
        setStatus(section, 'Loading photos...', 'info');

        try {
            const response = await fetch(section.dataset.listUrl, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!response.ok) {
                setStatus(section, 'Could not load photos.', 'error');
                return;
            }

            renderPhotos(section, await response.json());
        } catch {
            setStatus(section, 'Could not load photos. Check your connection and try again.', 'error');
        }
    }

    async function uploadWithFallbackInput(section, files) {
        if (!files.length) return;

        for (const file of files) {
            setStatus(section, `Uploading ${file.name}...`, 'info');
            const response = await fetch(section.dataset.uploadUrl, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken(section)
                },
                body: getUploadFormData(section, file)
            });

            if (!response.ok) {
                let message = 'Upload failed. Please try again.';
                try {
                    const body = await response.json();
                    message = body.error || message;
                } catch {
                    message = await response.text() || message;
                }

                setStatus(section, message, 'error');
                return;
            }

            setStatus(section, `${file.name} uploaded.`, 'success');
        }

        await loadPhotos(section);
    }

    function initFallbackUploader(section) {
        const dropzoneElement = section.querySelector('.destination-photo-dropzone');
        const fallbackInput = section.querySelector('.destination-photo-input');

        dropzoneElement.classList.add('destination-photo-dropzone-fallback');
        dropzoneElement.addEventListener('dragover', (event) => {
            event.preventDefault();
            dropzoneElement.classList.add('is-dragover');
        });
        dropzoneElement.addEventListener('dragleave', () => {
            dropzoneElement.classList.remove('is-dragover');
        });
        dropzoneElement.addEventListener('drop', async (event) => {
            event.preventDefault();
            dropzoneElement.classList.remove('is-dragover');
            await uploadWithFallbackInput(section, event.dataTransfer.files);
        });
        fallbackInput.addEventListener('change', async () => {
            await uploadWithFallbackInput(section, fallbackInput.files);
            fallbackInput.value = '';
        });
        dropzoneElement.addEventListener('paste', async (event) => {
            const files = getClipboardImageFiles(event.clipboardData?.items || []);
            if (!files.length) {
                return;
            }

            event.preventDefault();
            await uploadWithFallbackInput(section, files);
        });

        dropzoneElement.setAttribute('tabindex', '0');
        setStatus(section, 'Upload control ready. You can click, drag a file, or paste an image.', 'info');
    }

    function initDropzone(section) {
        const dropzoneElement = section.querySelector('.destination-photo-dropzone');
        const inputElement = section.querySelector('.destination-photo-input');
        const token = getAntiForgeryToken(section);

        if (dropzoneElement.dropzone) {
            dropzoneElement.dropzone.destroy();
        }

        const dropzone = new Dropzone(dropzoneElement, {
            url: section.dataset.uploadUrl,
            paramName: 'file',
            maxFilesize: 10,
            acceptedFiles: acceptedMimeTypes.join(','),
            addRemoveLinks: true,
            parallelUploads: 1,
            uploadMultiple: false,
            clickable: true,
            headers: {
                'RequestVerificationToken': token
            },
            init: function () {
                dropzoneElement.setAttribute('tabindex', '0');
                setStatus(section, 'Upload control ready. You can click, drag a file, or paste an image.', 'info');

                inputElement.addEventListener('change', () => {
                    const files = Array.from(inputElement.files || []);
                    files.forEach((file) => this.addFile(file));
                    inputElement.value = '';
                });

                this.on('sending', (file, xhr, formData) => {
                    formData.append('uploadSessionId', section.dataset.uploadSessionId || '');
                    if (section.dataset.destinationId) {
                        formData.append('id', section.dataset.destinationId);
                    }
                    setStatus(section, `Uploading ${file.name}...`, 'info');
                });

                this.on('uploadprogress', (file, progress) => {
                    setStatus(section, `Uploading ${file.name}: ${Math.round(progress)}%`, 'info');
                });

                this.on('success', async (file) => {
                    setStatus(section, `${file.name} uploaded.`, 'success');
                    await loadPhotos(section);
                    this.removeFile(file);
                });

                this.on('error', (file, response) => {
                    const message = typeof response === 'string' ? response : response?.error;
                    setStatus(section, message || 'Upload failed. Please try again.', 'error');
                });

                dropzoneElement.addEventListener('paste', async (event) => {
                    const files = getClipboardImageFiles(event.clipboardData?.items || []);
                    if (!files.length) {
                        return;
                    }

                    event.preventDefault();
                    files.forEach((file) => this.addFile(file));
                });
            }
        });

        return dropzone;
    }

    function initDestinationPhotos() {
        const sections = document.querySelectorAll('[data-destination-photos]');
        if (!sections.length) {
            return;
        }

        sections.forEach((section) => {
            if (window.Dropzone) {
                initDropzone(section);
            } else {
                initFallbackUploader(section);
            }

            loadPhotos(section);
        });
    }

    document.addEventListener('DOMContentLoaded', initDestinationPhotos);
})();
