# Semantic Routing Model

This document provides a comprehensive mapping of all available URLs in the application, including the controllers, actions, and views used for each route.

## Complete URL Routing Map

### 1. HomeController (`/`)

| HTTP Method | URL Pattern | Action | View | Description |
|-------------|------------|--------|------|-------------|
| GET | `/` | `Index` | `Home/Index.cshtml` | Landing page / Home |
| GET | `/Home/Index` | `Index` | `Home/Index.cshtml` | Home page (explicit route) |
| GET | `/Privacy` | `Privacy` | `Home/Privacy.cshtml` | Privacy policy page |
| GET | `/Home/Privacy` | `Privacy` | `Home/Privacy.cshtml` | Privacy page (explicit route) |
| GET | `/Error` | `Error` | `Shared/Error.cshtml` | Error page (500 errors) |

**Controller Class**: `HomeController`

---

## Route Summary by Resource

### 2. TripsController (`/travel`)

| HTTP Method | URL Pattern | Action | View | Parameters | Description |
|-------------|------------|--------|------|-----------|-------------|
| GET | `/travel` | `Index` | `Trips/Index.cshtml` | - | List all trips |
| GET | `/travel/{slug}` | `Details` | `Trips/Details.cshtml` | `slug` (string) | Trip details by URL-friendly slug |
| GET | `/travel/Details/{id}` | `DetailsByIdFallback` | `Trips/Details.cshtml` | `id` (int) | Trip details by ID (fallback, legacy route) |

**Controller Class**: `TripsController`

**Route Prefix**: `[Route("travel")]`

**Slug Format**: Auto-generated from trip name (e.g., "Italy Trip" → "italy-trip")

**Example URLs**:
- `/travel` - All trips
- `/travel/italy-trip` - Trip details for "Italy Trip"
- `/travel/Details/1` - Trip with ID 1 (legacy)

---

### 3. DestinationsController (`/Destinations`)

| HTTP Method | URL Pattern | Action | View | Parameters | Description |
|-------------|------------|--------|------|-----------|-------------|
| GET | `/Destinations` | `Index` | `Destinations/Index.cshtml` | - | List all destinations |
| GET | `/Destinations/{country}/{city}` | `Details` | `Destinations/Details.cshtml` | `country` (string), `city` (string) | Destination details by country and city |
| GET | `/Destinations/Details/{id}` | `DetailsByIdFallback` | `Destinations/Details.cshtml` | `id` (int) | Destination details by ID (fallback, legacy route) |

**Controller Class**: `DestinationsController`

**Route Prefix**: `[Route("Destinations")]`

**Search Matching**: Case-insensitive matching

**Example URLs**:
- `/Destinations` - All destinations
- `/Destinations/Italy/Rome` - Destination details for Rome, Italy
- `/Destinations/Details/1` - Destination with ID 1 (legacy)

---

### 4. ActivitiesController (`/activities`)

| HTTP Method | URL Pattern | Action | View | Parameters | Description |
|-------------|------------|--------|------|-----------|-------------|
| GET | `/activities` | `Index` | `Activities/Index.cshtml` | - | List all activities |
| GET | `/activities/{activityType}/{activityName}` | `Details` | `Activities/Details.cshtml` | `activityType` (enum), `activityName` (slug) | Activity details by type and name |
| GET | `/activities/Details/{id}` | `DetailsByIdFallback` | `Activities/Details.cshtml` | `id` (int) | Activity details by ID (fallback, legacy route) |

**Controller Class**: `ActivitiesController`

**Route Prefix**: `[Route("activities")]`

**Activity Types**: Food, Sightseeing, Party, Relaxation (enum values)

**Slug Format**: Auto-generated from activity name (e.g., "Sushi Tour" → "sushi-tour")

**Example URLs**:
- `/activities` - All activities
- `/activities/Food/sushi-tour` - Food activity: "Sushi Tour"
- `/activities/Sightseeing/colosseum-tour` - Sightseeing activity: "Colosseum Tour"
- `/activities/Details/1` - Activity with ID 1 (legacy)

---

### 5. AccommodationsController (`/stays`)

| HTTP Method | URL Pattern | Action | View | Parameters | Description |
|-------------|------------|--------|------|-----------|-------------|
| GET | `/stays` | `Index` | `Accommodations/Index.cshtml` | - | List all accommodations |
| GET | `/stays/{accommodationType}/{accommodationName}` | `Details` | `Accommodations/Details.cshtml` | `accommodationType` (enum), `accommodationName` (slug) | Accommodation details by type and name |
| GET | `/stays/Details/{id}` | `DetailsByIdFallback` | `Accommodations/Details.cshtml` | `id` (int) | Accommodation details by ID (fallback, legacy route) |

**Controller Class**: `AccommodationsController`

**Route Prefix**: `[Route("stays")]`

**Accommodation Types**: Hotel, Apartment, Hostel, Villa (enum values)

**Slug Format**: Auto-generated from accommodation name (e.g., "Hotel Roma" → "hotel-roma")

**Example URLs**:
- `/stays` - All accommodations
- `/stays/Hotel/hotel-roma` - Hotel: "Hotel Roma"
- `/stays/Apartment/cozy-studio` - Apartment: "Cozy Studio"
- `/stays/Details/1` - Accommodation with ID 1 (legacy)

---

### 6. ReviewsController (`/Reviews`)

| HTTP Method | URL Pattern | Action | View | Parameters | Description |
|-------------|------------|--------|------|-----------|-------------|
| GET | `/Reviews` | `Index` | `Reviews/Index.cshtml` | - | List all reviews (filter: All) |
| GET | `/Reviews/{id}` | `Details` | `Reviews/Details.cshtml` | `id` (int) | Single review details by ID |
| GET | `/Reviews/Recommended` | `Recommended` | `Reviews/Index.cshtml` | - | List recommended reviews (rating ≥ 4) |
| GET | `/Reviews/NeedsImprovements` | `NeedsImprovements` | `Reviews/Index.cshtml` | - | List reviews needing improvements (rating < 4) |

**Controller Class**: `ReviewsController`

**Route Prefix**: `[Route("Reviews")]`

**Filtering**: Reviews are filtered by rating (ViewData["CurrentFilter"] passed to view)

**Example URLs**:
- `/Reviews` - All reviews
- `/Reviews/1` - Review with ID 1
- `/Reviews/Recommended` - Reviews with rating ≥ 4
- `/Reviews/NeedsImprovements` - Reviews with rating < 4

---

### 7. TransportsController (`/Transports`)

| HTTP Method | URL Pattern | Action | View | Parameters | Description |
|-------------|------------|--------|------|-----------|-------------|
| GET | `/Transports` | `Index` | `Transports/Index.cshtml` | - | List all transport options |
| GET | `/Transports/Details/{id}` | `Details` | `Transports/Details.cshtml` | `id` (int) | Transport details by ID |

**Controller Class**: `TransportsController`

**Route Prefix**: `[Route("Transports")]`

**Example URLs**:
- `/Transports` - All transports
- `/Transports/Details/1` - Transport with ID 1

---

## URL Slug Generation

## Complete Views Map

This table shows all Views in the application and the routes/actions that use them:

| View Path | Controller | Action(s) | Used For |
|-----------|-----------|-----------|----------|
| `Home/Index.cshtml` | HomeController | Index | Landing page |
| `Home/Privacy.cshtml` | HomeController | Privacy | Privacy policy |
| `Shared/Error.cshtml` | HomeController | Error | Error display |
| `Trips/Index.cshtml` | TripsController | Index | List all trips |
| `Trips/Details.cshtml` | TripsController | Details, DetailsByIdFallback | Trip detail page with nested entities |
| `Destinations/Index.cshtml` | DestinationsController | Index | List all destinations |
| `Destinations/Details.cshtml` | DestinationsController | Details, DetailsByIdFallback | Destination detail page with related content |
| `Activities/Index.cshtml` | ActivitiesController | Index | List all activities |
| `Activities/Details.cshtml` | ActivitiesController | Details, DetailsByIdFallback | Activity detail page |
| `Accommodations/Index.cshtml` | AccommodationsController | Index | List all accommodations |
| `Accommodations/Details.cshtml` | AccommodationsController | Details, DetailsByIdFallback | Accommodation detail page |
| `Reviews/Index.cshtml` | ReviewsController | Index, Recommended, NeedsImprovements | List reviews with filtering |
| `Reviews/Details.cshtml` | ReviewsController | Details | Single review detail page |
| `Transports/Index.cshtml` | TransportsController | Index | List all transports |
| `Transports/Details.cshtml` | TransportsController | Details | Transport detail page |

---

## Slug Generation Algorithm
1. Convert text to lowercase
2. Remove special characters (keep only alphanumeric, hyphens, and spaces)
3. Replace spaces with hyphens
4. Remove consecutive hyphens
5. Trim leading/trailing hyphens

### Examples
- "Italy Trip" → "italy-trip"
- "Sushi Tour" → "sushi-tour"
- "Hotel Roma" → "hotel-roma"
- "My Awesome Trip!" → "my-awesome-trip"

## Slug Generation Algorithm

Implemented in `TripsController`, `ActivitiesController`, and `AccommodationsController`:

```csharp
private string GenerateSlug(string text)
{
    if (string.IsNullOrEmpty(text)) return "";
    var slug = Regex.Replace(text.ToLower(), @"[^a-z0-9\s-]", "");
    slug = Regex.Replace(slug, @"\s+", "-");
    return Regex.Replace(slug, @"-+", "-").Trim('-');
}
```

**Steps**:
1. Convert text to lowercase
2. Remove special characters (keep only alphanumeric, hyphens, and spaces)
3. Replace spaces with hyphens
4. Remove consecutive hyphens
5. Trim leading/trailing hyphens

**Examples**:
- "Italy Trip" → "italy-trip"
- "Sushi Tour" → "sushi-tour"
- "Hotel Roma" → "hotel-roma"
- "My Awesome Trip!" → "my-awesome-trip"
- "Colosseum-Tour" → "colosseum-tour"

---

## Backwards Compatibility

All resource controllers maintain a **fallback route** using the legacy `Details/{id}` pattern to ensure existing links and bookmarks continue to work:

- **Trips**: `/travel/Details/{id}` → Action: `DetailsByIdFallback`
- **Destinations**: `/Destinations/Details/{id}` → Action: `DetailsByIdFallback`
- **Activities**: `/activities/Details/{id}` → Action: `DetailsByIdFallback`
- **Accommodations**: `/stays/Details/{id}` → Action: `DetailsByIdFallback`
- **Reviews**: `/Reviews/{id}` → Action: `Details` (primary route)
- **Transports**: `/Transports/Details/{id}` → Action: `Details` (primary route)

---

## Route Attributes & Configuration

All controllers use **ASP.NET Core attribute routing**:

```csharp
[Route("...")] // Class-level: Sets the base route prefix for the controller
public class XyzController : Controller
{
    [HttpGet(...)] // Method-level: Defines the specific action route
    public IActionResult MethodName(string parameter) { ... }
}
```

### Route Prefix by Controller

| Controller | Route Prefix | Base URL |
|-----------|------------|----------|
| HomeController | (none - default) | `/` |
| TripsController | `[Route("travel")]` | `/travel` |
| DestinationsController | `[Route("Destinations")]` | `/Destinations` |
| ActivitiesController | `[Route("activities")]` | `/activities` |
| AccommodationsController | `[Route("stays")]` | `/stays` |
| ReviewsController | `[Route("Reviews")]` | `/Reviews` |
| TransportsController | `[Route("Transports")]` | `/Transports` |

---

## Entity Relationships & Data Loading

Each Details action uses **eager loading** with `Include()` and `ThenInclude()` to fetch related entities:

### Trips Details
- Loads: User, Destinations, Activities (per destination), Accommodations (per destination), Transports (per destination), Reviews (per destination)

### Destinations Details
- Loads: Trip, Activities, Accommodations, Transports, Reviews (with User)

### Activities Details
- Loads: Destination (with Trip)

### Accommodations Details
- Loads: Destination (with Trip)

### Reviews Details
- Loads: User, Destination (with Trip)

### Transports Details
- Loads: Destination (with Trip)

---

## View Integration & URL Generation

Views use **Razor tag helpers** to generate correct URLs:

```html
<!-- Destination link -->
<a asp-action="Details" asp-route-country="Italy" asp-route-city="Rome">View</a>
<!-- Generates: /Destinations/Italy/Rome -->

<!-- Trip link -->
<a asp-action="Details" asp-route-slug="italy-trip">View</a>
<!-- Generates: /travel/italy-trip -->

<!-- Activity link -->
<a asp-action="Details" asp-route-activityType="Food" asp-route-activityName="sushi-tour">View</a>
<!-- Generates: /activities/Food/sushi-tour -->

<!-- Accommodation link -->
<a asp-action="Details" asp-route-accommodationType="Hotel" asp-route-accommodationName="hotel-roma">View</a>
<!-- Generates: /stays/Hotel/hotel-roma -->

<!-- Review link -->
<a asp-action="Details" asp-route-id="1">View</a>
<!-- Generates: /Reviews/1 -->

<!-- Transport link -->
<a asp-action="Details" asp-route-id="1">View</a>
<!-- Generates: /Transports/Details/1 -->
```

---

## Common Parameter Matching Rules

| Route Type | Matching | Case Sensitivity | Example |
|-----------|----------|------------------|---------|
| Country/City (Destinations) | Exact text match | Case-insensitive | "Italy" or "italy" matches |
| Activity Type | Enum name | Case-insensitive | "Food" or "food" matches Food enum |
| Activity Name | Slug comparison | Case-insensitive slug | "Sushi-Tour" or "sushi-tour" matches slug |
| Accommodation Type | Enum name | Case-insensitive | "Hotel" or "hotel" matches Hotel enum |
| Accommodation Name | Slug comparison | Case-insensitive slug | "Hotel-Roma" or "hotel-roma" matches slug |
| Trip Slug | Slug comparison | Case-insensitive slug | "Italy-Trip" or "italy-trip" matches slug |
| ID-based (numeric) | Numeric ID | Exact | 1, 2, 3, etc. |

---

## Error Handling

- **404 Not Found**: Returned when a resource with matching parameters cannot be found
- **All failures use**: `return NotFound();` → Renders default 404 page
- **No redirect loops**: Both primary and fallback routes return the same view

---

## Summary of All Controllers & Actions

| # | Controller | Actions | Routes | Views |
|---|-----------|---------|--------|-------|
| 1 | HomeController | 3 | 4 URL patterns | 3 views |
| 2 | TripsController | 3 | 3 URL patterns | 1 detail view |
| 3 | DestinationsController | 3 | 3 URL patterns | 1 detail view |
| 4 | ActivitiesController | 3 | 3 URL patterns | 1 detail view |
| 5 | AccommodationsController | 3 | 3 URL patterns | 1 detail view |
| 6 | ReviewsController | 4 | 4 URL patterns | 2 views |
| 7 | TransportsController | 2 | 2 URL patterns | 1 detail view |
| **TOTAL** | **7 Controllers** | **21 Actions** | **23 Unique Patterns** | **14 Views** |

---

## Reference Tables

### HTTP Methods Used

| HTTP Method | Purpose | Used In |
|------------|---------|---------|
| GET | Retrieve resources (default for all routes) | All controllers |

### Response Types

| Response Type | When Used | HTTP Status |
|--------------|-----------|------------|
| `View(model)` | Render view with model data | 200 OK |
| `NotFound()` | Resource not found | 404 Not Found |

---

## Examples

### Example 1: Accessing a Destination
**URL**: `/Destinations/Italy/Rome`
- **Controller**: DestinationsController
- **Action**: Details(string country, string city)
- **Parameters**: country = "Italy", city = "Rome"
- **View**: Destinations/Details.cshtml
- **Data Returned**: Destination object with Trip, Activities, Accommodations, Transports, Reviews

### Example 2: Accessing a Trip by Slug
**URL**: `/travel/italy-trip`
- **Controller**: TripsController
- **Action**: Details(string slug)
- **Parameters**: slug = "italy-trip"
- **View**: Trips/Details.cshtml
- **Data Returned**: Trip object with all related Destinations and their nested entities

### Example 3: Filtered Reviews (Recommended)
**URL**: `/Reviews/Recommended`
- **Controller**: ReviewsController
- **Action**: Recommended()
- **View**: Reviews/Index.cshtml
- **Filter**: Reviews with rating ≥ 4, sorted descending by rating
- **ViewData**: CurrentFilter = "Recommended"
