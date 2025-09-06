# Configuration OAuth pour Google Sign-In et Apple Sign-In

Ce guide détaille comment obtenir et configurer les identifiants OAuth nécessaires pour faire fonctionner l'authentification Google et Apple dans votre application VideGreniers.

## 📋 Prérequis

- Compte Google Cloud Platform
- Compte Apple Developer Program (payant, 99$/an)
- Accès aux fichiers de configuration de l'API VideGreniers

---

## 🔵 Configuration Google Sign-In

### Étape 1 : Créer un Projet Google Cloud

1. Allez sur [Google Cloud Console](https://console.cloud.google.com/)
2. Cliquez sur "Nouveau projet" ou sélectionnez un projet existant
3. Nommez votre projet (ex: "VideGreniers-OAuth")
4. Notez l'ID du projet créé

### Étape 2 : Activer l'API Google Sign-In

1. Dans Google Cloud Console, allez dans "APIs & Services" > "Library"
2. Recherchez "Google Sign-In API" ou "Google+ API"
3. Cliquez sur "Enable"

### Étape 3 : Créer les Identifiants OAuth 2.0

1. Allez dans "APIs & Services" > "Credentials"
2. Cliquez sur "Create Credentials" > "OAuth client ID"
3. Si demandé, configurez l'écran de consentement OAuth :
   - Type d'application : External
   - Nom de l'application : "VideGreniers"
   - Email de support : votre email
   - Domaines autorisés : ajoutez votre domaine (ex: `videgreniers.com`)

### Étape 4 : Configurer le Client OAuth

1. **Pour l'application Web (Backend)** :
   - Type d'application : "Web application"
   - Nom : "VideGreniers Backend"
   - Origines JavaScript autorisées :
     ```
     http://localhost:5029
     https://localhost:7285
     https://votre-domaine.com
     ```
   - URI de redirection autorisés :
     ```
     http://localhost:5029/signin-google
     https://localhost:7285/signin-google
     https://votre-domaine.com/signin-google
     ```

2. **Pour l'application iOS** :
   - Créez un second client OAuth
   - Type d'application : "iOS"
   - Bundle ID : votre bundle ID iOS (ex: `com.votrecompagnie.videgreniers`)

### Étape 5 : Récupérer les Identifiants

Une fois créé, vous obtiendrez :
- **Client ID** : `123456789-abcdef.apps.googleusercontent.com`
- **Client Secret** : `GOCSPX-abcdefghijklmnop`

> ⚠️ **Sécurité** : Ne commitez jamais le Client Secret dans votre code source !

---

## 🍎 Configuration Apple Sign-In

### Étape 1 : Créer un App ID

1. Allez sur [Apple Developer Portal](https://developer.apple.com/)
2. Connectez-vous avec votre compte Developer
3. Allez dans "Certificates, Identifiers & Profiles"
4. Cliquez sur "Identifiers" puis "+" pour créer un nouvel identifiant
5. Sélectionnez "App IDs" puis "App"
6. Configuration :
   - Description : "VideGreniers iOS App"
   - Bundle ID : `com.votrecompagnie.videgreniers` (explicit)
   - Capabilities : Cochez "Sign In with Apple"

### Étape 2 : Créer un Service ID (pour le Backend)

1. Créez un nouvel identifiant
2. Sélectionnez "Services IDs"
3. Configuration :
   - Description : "VideGreniers Backend Service"
   - Identifier : `com.votrecompagnie.videgreniers.backend`
   - Cochez "Sign In with Apple"
4. Configurez "Sign In with Apple" :
   - Primary App ID : sélectionnez l'App ID créé précédemment
   - Domains and Subdomains : `votre-domaine.com`, `localhost` (pour dev)
   - Return URLs : 
     ```
     https://votre-domaine.com/signin-apple
     http://localhost:5029/signin-apple
     ```

### Étape 3 : Créer une Clé Privée

1. Allez dans "Keys"
2. Cliquez sur "+" pour créer une nouvelle clé
3. Configuration :
   - Key Name : "VideGreniers Sign In with Apple Key"
   - Cochez "Sign In with Apple"
   - Configurez avec votre App ID principal
4. Téléchargez le fichier `.p8` généré
5. **IMPORTANT** : Notez le Key ID (ex: `AB1C2D3E4F`) - vous ne pourrez plus le voir

### Étape 4 : Récupérer les Informations Nécessaires

Vous aurez besoin de :
- **Team ID** : Trouvé dans "Membership" de votre compte Developer (ex: `1A2B3C4D5E`)
- **Client ID** : L'identifier de votre Service ID (ex: `com.votrecompagnie.videgreniers.backend`)
- **Key ID** : L'identifiant de la clé privée créée (ex: `AB1C2D3E4F`)
- **Private Key File** : Le fichier `.p8` téléchargé

---

## ⚙️ Configuration de l'Application

### Mise à jour des fichiers appsettings

#### appsettings.Development.json
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "123456789-abcdefghijklmnop.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-votre-client-secret-google"
    },
    "Apple": {
      "TeamId": "1A2B3C4D5E",
      "ClientId": "com.votrecompagnie.videgreniers.backend",
      "KeyId": "AB1C2D3E4F"
    }
  }
}
```

#### appsettings.json (Production)
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "GOOGLE_CLIENT_ID_FROM_ENV",
      "ClientSecret": "GOOGLE_CLIENT_SECRET_FROM_ENV"
    },
    "Apple": {
      "TeamId": "APPLE_TEAM_ID_FROM_ENV",
      "ClientId": "APPLE_CLIENT_ID_FROM_ENV",
      "KeyId": "APPLE_KEY_ID_FROM_ENV"
    }
  }
}
```

### Variables d'Environnement (Production)

Pour la production, utilisez des variables d'environnement :

```bash
export GOOGLE_CLIENT_ID="123456789-abcdef.apps.googleusercontent.com"
export GOOGLE_CLIENT_SECRET="GOCSPX-votre-secret"
export APPLE_TEAM_ID="1A2B3C4D5E"
export APPLE_CLIENT_ID="com.votrecompagnie.videgreniers.backend"
export APPLE_KEY_ID="AB1C2D3E4F"
```

Ou dans un fichier `.env` :
```env
GOOGLE_CLIENT_ID=123456789-abcdef.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=GOCSPX-votre-secret
APPLE_TEAM_ID=1A2B3C4D5E
APPLE_CLIENT_ID=com.votrecompagnie.videgreniers.backend
APPLE_KEY_ID=AB1C2D3E4F
```

---

## 📱 Configuration iOS

### Fichier GoogleService-Info.plist (Google)

1. Dans Google Cloud Console, allez dans votre client iOS
2. Téléchargez le fichier `GoogleService-Info.plist`
3. Ajoutez-le à votre projet iOS Xcode
4. Assurez-vous qu'il soit inclus dans le bundle

### Info.plist Configuration (Apple)

Ajoutez ceci à votre `Info.plist` iOS :

```xml
<key>CFBundleURLTypes</key>
<array>
    <!-- Google Sign-In -->
    <dict>
        <key>CFBundleURLName</key>
        <string>google-signin</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>123456789-abcdefghijklmnop.apps.googleusercontent.com</string>
        </array>
    </dict>
    <!-- Apple Sign-In -->
    <dict>
        <key>CFBundleURLName</key>
        <string>apple-signin</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>com.votrecompagnie.videgreniers</string>
        </array>
    </dict>
</array>
```

---

## 🧪 Test de Configuration

### Test Google Sign-In

1. Démarrez votre API : `dotnet run --project VideGreniers.API`
2. Utilisez curl pour tester avec un vrai token Google :

```bash
curl -X POST "http://localhost:5029/api/auth/oauth/google" \
-H "Content-Type: application/json" \
-d '{
  "idToken": "eyJhbGciOiJSUzI1NiIs..."
}'
```

### Test Apple Sign-In

```bash
curl -X POST "http://localhost:5029/api/auth/oauth/apple" \
-H "Content-Type: application/json" \
-d '{
  "identityToken": "eyJraWQiOiJmaDZCcz...",
  "userFirstName": "John",
  "userLastName": "Doe"
}'
```

---

## 🚨 Troubleshooting

### Erreurs Courantes Google

#### "Invalid client ID"
- Vérifiez que le Client ID est correct dans appsettings
- Assurez-vous que les origines sont bien configurées dans Google Cloud Console

#### "Token verification failed"
- Le token Google est peut-être expiré
- Vérifiez que l'horloge du serveur est synchronisée

### Erreurs Courantes Apple

#### "Invalid client ID"
- Vérifiez que le Service ID existe dans Apple Developer Portal
- Assurez-vous que "Sign In with Apple" est activé

#### "Key not found"
- Vérifiez que le Key ID est correct
- Assurez-vous que la clé privée `.p8` est accessible (pour les versions futures avec validation locale)

#### "Invalid token"
- Le token Apple expire très rapidement (quelques minutes)
- Assurez-vous que l'horloge du serveur est précise

### Logs de Débogage

Activez les logs détaillés dans `appsettings.Development.json` :

```json
{
  "Logging": {
    "LogLevel": {
      "VideGreniers.Infrastructure.Identity.OAuth": "Debug",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

---

## 📚 Ressources Utiles

### Documentation Officielle
- [Google Sign-In for Websites](https://developers.google.com/identity/sign-in/web)
- [Apple Sign-In Documentation](https://developer.apple.com/documentation/sign_in_with_apple)
- [ASP.NET Core OAuth Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/)

### Outils de Test
- [Google OAuth Playground](https://developers.google.com/oauthplayground/)
- [JWT.io](https://jwt.io/) pour décoder les tokens
- [Apple JWT Decoder](https://appleid.apple.com/) pour les tokens Apple

---

## ⚠️ Sécurité

### Bonnes Pratiques

1. **Secrets** : Jamais de secrets dans le code source
2. **Variables d'environnement** : Utilisez-les en production
3. **HTTPS** : Obligatoire en production pour OAuth
4. **Rotation des clés** : Changez régulièrement vos secrets
5. **Validation** : Toujours valider les tokens côté serveur

### Fichiers à ne JAMAIS commiter

- `appsettings.Production.json` avec de vraies clés
- Fichiers `.p8` d'Apple
- `GoogleService-Info.plist` avec de vraies clés
- Fichiers `.env` avec des secrets

---

*Guide créé le 6 septembre 2025 pour VideGreniers v1.2*