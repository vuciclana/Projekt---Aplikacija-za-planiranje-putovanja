# Semantic Database Model

## Entities / Tables

### Users
- `Id` (PK)
- `FirstName`
- `LastName`
- `Email`

### Trips
- `Id` (PK)
- `Name`
- `StartDate`
- `EndDate`
- `UserId` (FK -> Users)

### Destinations
- `Id` (PK)
- `City`
- `Country`
- `Description`
- `TripId` (FK -> Trips)

### Activities
- `Id` (PK)
- `Name`
- `TypeOfActivity`
- `Date`
- `Cost`
- `DestinationId` (FK -> Destinations)

### Accommodations
- `Id` (PK)
- `Name`
- `Type`
- `Address`
- `CostPerNight`
- `CheckInDate`
- `CheckOutDate`
- `DestinationId` (FK -> Destinations)

### Transports
- `Id` (PK)
- `Type`
- `Cost`
- `DepartureTime`
- `ArrivalTime`
- `DestinationId` (FK -> Destinations)

### Reviews
- `Id` (PK)
- `Rating`
- `Comment`
- `UserId` (FK -> Users)
- `DestinationId` (FK -> Destinations)

## Relationships

- `User` 1:N `Trips`.
- `Trip` 1:N `Destinations`.
- `Destination` 1:N `Activities`.
- `Destination` 1:N `Accommodations`.
- `Destination` 1:N `Transports`.
- `Destination` 1:N `Reviews`.
- `User` 1:N `Reviews`.

## Derived / Computed Fields

The following fields are calculated in code and are not necessarily stored as database columns:

- `Trip.TotalCost`
- `Destination.TotalDestinationCost`
- `Accommodation.NumberOfNights`
- `Accommodation.TotalCost`
- `Transport.Duration`
- `Review.Stars`
