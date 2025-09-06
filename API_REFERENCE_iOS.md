# VideGreniers API Reference for iOS

## Overview

**Base URL:** 
- Development: `http://localhost:5030` / `https://localhost:7285`
- Production: `https://your-production-domain.com`

**Authentication:** JWT Bearer Token
**Content Type:** `application/json`

## Quick Start

1. Register a new user or login to get access tokens
2. Use Bearer token for authenticated endpoints
3. Refresh tokens before expiration (15 minutes for access tokens)

---

## 🔐 Authentication Endpoints

### Base Route: `/api/auth`

#### 1. Register
**POST** `/api/auth/register`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John", // optional
  "lastName": "Doe"    // optional
}
```

**Response:**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "def502004a1b2c3d...",
    "expiresIn": 900, // 15 minutes
    "user": {
      "id": "uuid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    }
  },
  "success": true,
  "timestamp": "2023-09-06T13:00:00Z"
}
```

#### 2. Login
**POST** `/api/auth/login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** Same as register

#### 3. Refresh Token
**POST** `/api/auth/refresh`

**Request:**
```json
{
  "accessToken": "current-access-token",
  "refreshToken": "current-refresh-token"
}
```

#### 4. Logout
**POST** `/api/auth/logout` 🔒

**Headers:** `Authorization: Bearer {access-token}`

**Request:**
```json
{
  "refreshToken": "current-refresh-token"
}
```

#### 5. Current User Info
**GET** `/api/auth/me` 🔒

**Response:**
```json
{
  "data": {
    "id": "uuid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"],
    "isAuthenticated": true
  },
  "success": true
}
```

#### 6. Google OAuth (Coming Soon)
**POST** `/api/auth/oauth/google`

---

## 📅 Events Endpoints

### Base Route: `/api/events`

#### 1. Get Events (Public)
**GET** `/api/events`

**Query Parameters:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20, max: 100) - Items per page
- `searchTerm` (string) - Search in title/description
- `categoryId` (guid) - Filter by category
- `eventType` (int) - Filter by event type (see EventType enum)
- `city` (string) - Filter by city
- `startDate` (datetime) - Events starting after this date
- `endDate` (datetime) - Events starting before this date
- `hasEntryFee` (bool) - Filter by entry fee presence
- `sortBy` (string, default: "StartDate") - Sort field
- `sortDescending` (bool, default: false) - Sort direction

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "title": "Grande Brocante du Village",
      "description": "Vide-grenier annuel avec plus de 50 exposants...",
      "location": {
        "street": "Place de la Mairie",
        "city": "Bourg-en-Bresse",
        "postalCode": "01000",
        "country": "France",
        "state": null,
        "latitude": 46.2057,
        "longitude": 5.2281
      },
      "startDate": "2023-09-15T08:00:00Z",
      "endDate": "2023-09-15T18:00:00Z",
      "status": 1, // Published
      "eventType": 1, // FleaMarket
      "contactEmail": "contact@mairie-village.fr",
      "contactPhone": "+33123456789",
      "specialInstructions": "Parking gratuit disponible",
      "entryFeeAmount": 2.00,
      "entryFeeCurrency": "EUR",
      "allowsEarlyBird": true,
      "earlyBirdTime": "07:30:00",
      "earlyBirdFeeAmount": 5.00,
      "earlyBirdFeeCurrency": "EUR",
      "publishedOnUtc": "2023-09-01T10:00:00Z",
      "createdOnUtc": "2023-08-25T14:30:00Z",
      "modifiedOnUtc": "2023-09-01T10:00:00Z",
      "organizerName": "Mairie du Village",
      "categoryName": "Brocante",
      "categoryIcon": "🏛️",
      "categoryColor": "#4CAF50",
      "distanceKm": 12.5, // Only for nearby searches
      "isFavorite": false, // Only when user is authenticated
      "favoriteCount": 47
    }
  ],
  "success": true,
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 3,
    "totalCount": 52,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

#### 2. Get Event by ID (Public)
**GET** `/api/events/{id}`

**Response:** Single EventDto object

#### 3. Get Nearby Events (Public)
**GET** `/api/events/nearby`

**Query Parameters:**
- `latitude` (double) - Required
- `longitude` (double) - Required  
- `radiusKm` (int, default: 10, max: 100) - Search radius
- `limit` (int, default: 20, max: 50) - Max results

**Response:** List of EventDto with distance information

#### 4. Create Event
**POST** `/api/events` 🔒

**Request:** CreateEventCommand object (see Command DTOs section)

#### 5. Update Event
**PUT** `/api/events/{id}` 🔒

**Request:** UpdateEventCommand object

#### 6. Publish Event
**POST** `/api/events/{id}/publish` 🔒

#### 7. Cancel Event
**POST** `/api/events/{id}/cancel` 🔒

**Request:**
```json
{
  "reason": "Météo défavorable"
}
```

---

## ⭐ Favorites Endpoints

### Base Route: `/api/favorites` 🔒

#### 1. Get User Favorites
**GET** `/api/favorites`

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)

**Response:** Paginated list of FavoriteDto

#### 2. Add to Favorites
**POST** `/api/favorites/{eventId}`

#### 3. Remove from Favorites
**DELETE** `/api/favorites/{eventId}`

#### 4. Toggle Favorite Status
**PATCH** `/api/favorites/{eventId}/toggle`

**Response:**
```json
{
  "data": {
    "eventId": "uuid",
    "isFavorite": true,
    "action": "added" // or "removed"
  },
  "success": true
}
```

#### 5. Check Favorite Status
**POST** `/api/favorites/status`

**Request:**
```json
["event-uuid-1", "event-uuid-2", "event-uuid-3"]
```

**Response:**
```json
{
  "data": {
    "event-uuid-1": true,
    "event-uuid-2": false,
    "event-uuid-3": true
  },
  "success": true
}
```

#### 6. Get Upcoming Favorites
**GET** `/api/favorites/upcoming`

**Query Parameters:**
- `daysAhead` (int, default: 7) - Look ahead days

---

## 🔔 Notifications Endpoints

### Base Route: `/api/notifications` 🔒

#### 1. Get User Notifications
**GET** `/api/notifications`

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `unreadOnly` (bool, default: false)

**Response:** Paginated list of NotificationDto

#### 2. Mark Notification as Read
**PATCH** `/api/notifications/{notificationId}/read`

#### 3. Create Notification (Admin Only)
**POST** `/api/notifications` 🔒👑

---

## 👤 User Endpoints

### Base Route: `/api/user` 🔒

#### 1. Get Profile
**GET** `/api/user/profile`

**Response:**
```json
{
  "data": {
    "id": "uuid",
    "firstName": "John",
    "lastName": "Doe", 
    "email": "john.doe@example.com",
    "phoneNumber": "+33123456789",
    "createdOnUtc": "2023-01-15T10:30:00Z",
    "modifiedOnUtc": "2023-08-20T14:20:00Z",
    "fullName": "John Doe",
    "createdEventsCount": 3,
    "favoritesCount": 12
  },
  "success": true
}
```

#### 2. Update Profile
**PUT** `/api/user/profile`

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+33123456789",
  "bio": "Passionné de vide-greniers depuis 20 ans",
  "avatarUrl": "https://example.com/avatar.jpg",
  "preferredLanguage": "fr-FR",
  "emailNotifications": true,
  "pushNotifications": true,
  "smsNotifications": false
}
```

#### 3. Get Account Statistics
**GET** `/api/user/stats`

**Response:**
```json
{
  "data": {
    "totalFavorites": 12,
    "totalEventsCreated": 3,
    "totalNotifications": 25,
    "unreadNotifications": 4,
    "accountCreatedDate": "2023-01-15T10:30:00Z",
    "lastLoginDate": "2023-09-06T08:15:00Z",
    "daysActive": 234
  },
  "success": true
}
```

#### 4. Delete Account
**DELETE** `/api/user/account`

---

## 📊 User Activities Endpoints

### Base Route: `/api/user-activities` 🔒

#### 1. Get User Activities
**GET** `/api/user-activities`

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)
- `activityType` (UserActivityType): Filter by activity type
- `startDate` (DateTime): Filter activities after this date
- `endDate` (DateTime): Filter activities before this date

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "activityType": 0,
      "eventId": "uuid",
      "eventTitle": "Grande Brocante de Paris",
      "searchTerm": null,
      "metadata": null,
      "createdOnUtc": "2023-09-06T10:30:00Z"
    }
  ],
  "success": true,
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalCount": 87,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "timestamp": "2023-09-06T13:00:00Z"
}
```

#### 2. Get Activity Statistics
**GET** `/api/user-activities/statistics`

**Query Parameters:**
- `startDate` (DateTime): Statistics period start date (default: 30 days ago)
- `endDate` (DateTime): Statistics period end date (default: now)

**Response:**
```json
{
  "data": {
    "totalActivities": 127,
    "activityCounts": {
      "0": 45,  // EventViewed
      "1": 12,  // EventFavorited
      "7": 8    // EventSearched
    },
    "eventsViewed": 45,
    "eventsFavorited": 12,
    "eventsCreated": 2,
    "searchesPerformed": 8,
    "topSearchTerms": ["brocante", "vintage", "antiquités"],
    "lastActivityDate": "2023-09-06T12:45:00Z",
    "periodStart": "2023-08-07T13:00:00Z",
    "periodEnd": "2023-09-06T13:00:00Z"
  },
  "success": true,
  "timestamp": "2023-09-06T13:00:00Z"
}
```

---

## 🏥 Health Check

**GET** `/health`

**Response:**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "cache": "Healthy"
  }
}
```

---

## 📋 Data Models (DTOs)

### EventDto
```swift
struct EventDto: Codable {
    let id: UUID
    let title: String
    let description: String
    let location: LocationDto
    let startDate: Date
    let endDate: Date
    let status: EventStatus
    let eventType: EventType
    let contactEmail: String?
    let contactPhone: String?
    let specialInstructions: String?
    let entryFeeAmount: Decimal?
    let entryFeeCurrency: String?
    let allowsEarlyBird: Bool
    let earlyBirdTime: String? // "HH:mm:ss" format
    let earlyBirdFeeAmount: Decimal?
    let earlyBirdFeeCurrency: String?
    let publishedOnUtc: Date?
    let createdOnUtc: Date
    let modifiedOnUtc: Date?
    
    // Navigation properties
    let organizerName: String?
    let categoryName: String?
    let categoryIcon: String?
    let categoryColor: String?
    
    // Computed properties
    let distanceKm: Double?
    let isFavorite: Bool?
    let favoriteCount: Int
}
```

### LocationDto
```swift
struct LocationDto: Codable {
    let street: String
    let city: String
    let postalCode: String
    let country: String
    let state: String?
    let latitude: Double
    let longitude: Double
}
```

### UserDto
```swift
struct UserDto: Codable {
    let id: UUID
    let firstName: String
    let lastName: String
    let email: String
    let phoneNumber: String?
    let createdOnUtc: Date
    let modifiedOnUtc: Date?
    
    // Computed
    let fullName: String
    let createdEventsCount: Int
    let favoritesCount: Int
}
```

### FavoriteDto
```swift
struct FavoriteDto: Codable {
    let id: UUID
    let userId: UUID
    let eventId: UUID
    let event: EventDto
    let createdOnUtc: Date
    let status: FavoriteStatus
}
```

### NotificationDto
```swift
struct NotificationDto: Codable {
    let id: UUID
    let title: String
    let message: String
    let type: NotificationType
    let status: NotificationStatus
    let readOnUtc: Date?
    let sentOnUtc: Date?
    let actionUrl: String?
    let actionText: String?
    let imageUrl: String?
    let createdOnUtc: Date
    
    // Navigation
    let eventId: UUID?
    let eventTitle: String?
    
    // Computed
    let isUnread: Bool
    let typeDisplayName: String
}
```

### UserStatsDto
```swift
struct UserStatsDto: Codable {
    let totalFavorites: Int
    let totalEventsCreated: Int
    let totalNotifications: Int
    let unreadNotifications: Int
    let accountCreatedDate: Date
    let lastLoginDate: Date?
    let daysActive: Int
}
```

### UserActivityDto
```swift
struct UserActivityDto: Codable {
    let id: UUID
    let activityType: UserActivityType
    let eventId: UUID?
    let eventTitle: String?
    let searchTerm: String?
    let metadata: String?
    let createdOnUtc: Date
}
```

### UserActivityStatisticsDto
```swift
struct UserActivityStatisticsDto: Codable {
    let totalActivities: Int
    let activityCounts: [UserActivityType: Int]
    let eventsViewed: Int
    let eventsFavorited: Int
    let eventsCreated: Int
    let searchesPerformed: Int
    let topSearchTerms: [String]
    let lastActivityDate: Date?
    let periodStart: Date
    let periodEnd: Date
    
    // Helper computed properties
    var averageActivitiesPerDay: Double {
        let days = Calendar.current.dateComponents([.day], from: periodStart, to: periodEnd).day ?? 1
        return Double(totalActivities) / Double(max(days, 1))
    }
    
    var mostFrequentActivity: UserActivityType? {
        activityCounts.max(by: { $0.value < $1.value })?.key
    }
}
```

---

## 🔢 Enumerations

### EventStatus
```swift
enum EventStatus: Int, Codable, CaseIterable {
    case draft = 0      // Brouillon
    case published = 1  // Publié
    case active = 2     // En cours
    case completed = 3  // Terminé
    case cancelled = 4  // Annulé
    case postponed = 5  // Reporté
}
```

### EventType
```swift
enum EventType: Int, Codable, CaseIterable {
    case garageSale = 0     // Vide-grenier particulier
    case fleaMarket = 1     // Brocante organisée
    case estateSale = 2     // Vente de succession
    case churchSale = 3     // Vente paroissiale
    case schoolSale = 4     // Vente école
    case movingSale = 5     // Vente déménagement
    case communityWide = 6  // Vide-grenier communal
    case specialized = 7    // Vente spécialisée
    case other = 99         // Autre
}
```

### NotificationType
```swift
enum NotificationType: Int, Codable {
    case system = 0         // Système
    case event = 1          // Événement
    case eventReminder = 2  // Rappel
    case favorite = 3       // Favoris
    case account = 4        // Compte
    case marketing = 5      // Marketing
}
```

### NotificationStatus
```swift
enum NotificationStatus: Int, Codable {
    case pending = 0    // En attente
    case sent = 1       // Envoyé
    case read = 2       // Lu
    case failed = 3     // Échec
}
```

### FavoriteStatus
```swift
enum FavoriteStatus: Int, Codable {
    case active = 0     // Actif
    case archived = 1   // Archivé
}
```

### UserActivityType
```swift
enum UserActivityType: Int, Codable, CaseIterable {
    case eventViewed = 0        // Événement consulté
    case eventFavorited = 1     // Événement mis en favoris
    case eventUnfavorited = 2   // Événement retiré des favoris
    case eventCreated = 3       // Événement créé
    case eventUpdated = 4       // Événement mis à jour
    case eventPublished = 5     // Événement publié
    case eventCancelled = 6     // Événement annulé
    case eventSearched = 7      // Recherche d'événements
    case userLogin = 8          // Connexion utilisateur
    case userLogout = 9         // Déconnexion utilisateur
    case profileUpdated = 10    // Profil mis à jour
    case searchSaved = 11       // Recherche sauvegardée
    case savedSearchExecuted = 12 // Recherche sauvegardée exécutée
    
    var displayName: String {
        switch self {
        case .eventViewed: return "Événement consulté"
        case .eventFavorited: return "Ajout aux favoris"
        case .eventUnfavorited: return "Retrait des favoris"
        case .eventCreated: return "Événement créé"
        case .eventUpdated: return "Événement modifié"
        case .eventPublished: return "Événement publié"
        case .eventCancelled: return "Événement annulé"
        case .eventSearched: return "Recherche effectuée"
        case .userLogin: return "Connexion"
        case .userLogout: return "Déconnexion"
        case .profileUpdated: return "Profil mis à jour"
        case .searchSaved: return "Recherche sauvée"
        case .savedSearchExecuted: return "Recherche sauvée exécutée"
        }
    }
    
    var isEventRelated: Bool {
        switch self {
        case .eventViewed, .eventFavorited, .eventUnfavorited,
             .eventCreated, .eventUpdated, .eventPublished, .eventCancelled:
            return true
        default:
            return false
        }
    }
}
```

---

## 📡 API Response Format

### Standard Response
```swift
struct ApiResponse<T: Codable>: Codable {
    let data: T?
    let success: Bool
    let message: String?
    let errors: [String]?
    let timestamp: Date
}
```

### Paginated Response
```swift
struct PaginatedApiResponse<T: Codable>: Codable {
    let data: [T]
    let success: Bool
    let message: String?
    let errors: [String]?
    let timestamp: Date
    let pagination: PaginationMetadata
}

struct PaginationMetadata: Codable {
    let page: Int
    let pageSize: Int
    let totalPages: Int
    let totalCount: Int
    let hasPreviousPage: Bool
    let hasNextPage: Bool
}
```

---

## 🔐 Authentication Guide for iOS

### JWT Token Storage
```swift
// Secure storage recommendation
import KeychainServices

class AuthTokenManager {
    private let keychain = Keychain(service: "com.yourapp.videGreniers")
    
    func saveTokens(accessToken: String, refreshToken: String) {
        keychain["accessToken"] = accessToken
        keychain["refreshToken"] = refreshToken
    }
    
    func getAccessToken() -> String? {
        return keychain["accessToken"]
    }
    
    func getRefreshToken() -> String? {
        return keychain["refreshToken"]
    }
}
```

### URLRequest Extension
```swift
extension URLRequest {
    mutating func addBearerToken(_ token: String) {
        setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
    }
}
```

### Auto Token Refresh
```swift
class APIClient {
    func makeRequest<T: Codable>(
        endpoint: String,
        method: HTTPMethod,
        body: Data? = nil,
        responseType: T.Type
    ) async throws -> T {
        
        var request = URLRequest(url: URL(string: baseURL + endpoint)!)
        request.httpMethod = method.rawValue
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = body
        
        // Add auth token
        if let token = AuthTokenManager.shared.getAccessToken() {
            request.addBearerToken(token)
        }
        
        let (data, response) = try await URLSession.shared.data(for: request)
        
        // Check for 401 and refresh token
        if let httpResponse = response as? HTTPURLResponse, 
           httpResponse.statusCode == 401 {
            try await refreshTokenIfNeeded()
            // Retry request with new token
            if let newToken = AuthTokenManager.shared.getAccessToken() {
                request.addBearerToken(newToken)
                let (newData, _) = try await URLSession.shared.data(for: request)
                return try JSONDecoder().decode(ApiResponse<T>.self, from: newData).data!
            }
        }
        
        return try JSONDecoder().decode(ApiResponse<T>.self, from: data).data!
    }
}
```

---

## 🍎 iOS-Specific Notes

### Sign in with Apple Integration
- OAuth endpoint en préparation: `/api/auth/oauth/apple`
- Utilisez `ASAuthorizationAppleIDProvider` côté iOS
- Envoyez l'`identityToken` au backend pour validation

### MapKit Integration
```swift
import MapKit

extension EventDto {
    var coordinate: CLLocationCoordinate2D {
        CLLocationCoordinate2D(
            latitude: location.latitude,
            longitude: location.longitude
        )
    }
    
    var mapItem: MKMapItem {
        let placemark = MKPlacemark(coordinate: coordinate)
        let mapItem = MKMapItem(placemark: placemark)
        mapItem.name = title
        return mapItem
    }
}
```

### Push Notifications
- Enregistrez le device token via un futur endpoint
- Gérez les notifications de rappel d'événements
- Intégrez avec `UNUserNotificationCenter`

### Core Location
```swift
// Pour la recherche d'événements à proximité
import CoreLocation

class LocationManager: NSObject, CLLocationManagerDelegate {
    private let locationManager = CLLocationManager()
    
    func requestLocationPermission() {
        locationManager.requestWhenInUseAuthorization()
    }
    
    func getCurrentLocation() async -> CLLocation? {
        // Implementation pour obtenir la position actuelle
    }
}
```

### User Activity Tracking & Analytics
Le système de tracking d'activité utilisateur fonctionne automatiquement côté backend. Voici comment l'intégrer dans votre app iOS :

```swift
// Service pour analyser et afficher les statistiques utilisateur
class UserActivityService: ObservableObject {
    @Published var activities: [UserActivityDto] = []
    @Published var statistics: UserActivityStatisticsDto?
    
    func fetchUserActivities(page: Int = 1, activityType: UserActivityType? = nil) async {
        // Appel GET /api/user-activities
        var urlComponents = URLComponents(string: "\(API_BASE_URL)/api/user-activities")!
        urlComponents.queryItems = [
            URLQueryItem(name: "page", value: String(page)),
            URLQueryItem(name: "pageSize", value: "20")
        ]
        
        if let activityType = activityType {
            urlComponents.queryItems?.append(
                URLQueryItem(name: "activityType", value: String(activityType.rawValue))
            )
        }
        
        // Implement API call logic...
    }
    
    func fetchStatistics(days: Int = 30) async {
        let endDate = Date()
        let startDate = Calendar.current.date(byAdding: .day, value: -days, to: endDate)!
        
        // Appel GET /api/user-activities/statistics
        // Implementation...
    }
}

// Vue pour afficher les statistiques
struct UserStatsView: View {
    @StateObject private var activityService = UserActivityService()
    
    var body: some View {
        VStack(alignment: .leading, spacing: 16) {
            if let stats = activityService.statistics {
                StatCard(title: "Événements vus", value: "\(stats.eventsViewed)")
                StatCard(title: "Favoris ajoutés", value: "\(stats.eventsFavorited)")
                StatCard(title: "Recherches", value: "\(stats.searchesPerformed)")
                
                // Graphique d'activité quotidienne
                Text("Activité moyenne: \(String(format: "%.1f", stats.averageActivitiesPerDay))/jour")
            }
        }
        .task {
            await activityService.fetchStatistics()
        }
    }
}
```

**Note sur la confidentialité :**
- Le tracking se fait automatiquement côté serveur lors des appels API
- Aucune donnée personnelle sensible n'est collectée
- Les adresses IP et User-Agent sont stockées uniquement pour les statistiques
- Respectez les guidelines d'Apple sur la transparence des données

---

## ⚠️ Error Handling

### HTTP Status Codes
- `200` - Success
- `201` - Created
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid/expired token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `409` - Conflict (duplicate resource)
- `429` - Rate Limited
- `500` - Server Error

### Error Response Format
```json
{
  "data": null,
  "success": false,
  "errors": [
    "Le champ Email est requis.",
    "Le mot de passe doit contenir au moins 8 caractères."
  ],
  "timestamp": "2023-09-06T13:00:00Z"
}
```

### Swift Error Handling
```swift
enum APIError: Error, LocalizedError {
    case unauthorized
    case notFound
    case validationError([String])
    case serverError(String)
    
    var errorDescription: String? {
        switch self {
        case .unauthorized:
            return "Session expirée. Veuillez vous reconnecter."
        case .notFound:
            return "Ressource introuvable."
        case .validationError(let errors):
            return errors.joined(separator: "\n")
        case .serverError(let message):
            return "Erreur serveur: \(message)"
        }
    }
}
```

---

## 🚀 Rate Limiting

- Authentification: 5 requêtes / minute / IP
- Général: 100 requêtes / minute / utilisateur authentifié
- Public: 60 requêtes / minute / IP

### Headers de Rate Limiting
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1693920000
```

---

## 📱 iOS Development Tips

### Networking Best Practices
1. Utilisez `async/await` pour les appels API
2. Implémentez un système de cache avec `NSCache`
3. Gérez la connectivité réseau avec `NWPathMonitor`
4. Utilisez `URLSession.shared.configuration.waitsForConnectivity`

### UI/UX Considerations
1. Affichez des states de chargement pendant les requêtes
2. Implémentez un refresh pull-to-refresh
3. Gérez les cas d'erreur avec des messages utilisateur friendly
4. Utilisez des images placeholder pendant le chargement

### Performance
1. Implémentez la pagination pour les listes d'événements
2. Utilisez des images optimisées (WebP si possible)
3. Mettez en cache les données fréquemment accédées
4. Implémentez un système de favoris offline

---

## 📞 Support & Documentation

- **Swagger UI:** `https://localhost:7285/swagger` (développement)
- **Postman Collection:** Disponible sur demande
- **Contact Support:** Créez une issue sur le repository

---

*Document généré le 6 septembre 2023 pour l'API VideGreniers v1.0*