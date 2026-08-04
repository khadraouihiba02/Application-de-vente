# ✈️ Application de Gestion des Ventes à Bord (VAB) — TUNISAIR Catering

Bienvenue sur le projet **Application de Gestion des Ventes à Bord**, une solution web complète développée pour la gestion, le contrôle et le traitement des ventes, offres de catering, redevances et commissions PNC de TUNISAIR.

---

## 📌 Présentation du Projet

L'application permet de numériser et d'automatiser l'ensemble du processus de gestion des ventes à bord des avions :
- **Saisie des états de ventes et offres** par les agents à bord.
- **Rapprochement et contrôle** des déclarations Agent / Fournisseur (FRS).
- **Calcul automatique des commissions PNC** (15% des ventes).
- **Calcul automatique de la Redevance Mensuelle** selon les règles du cahier des charges TUNISAIR (Minimum garanti vs 42% du CA).
- **Gestion comptable et financière (DCF)** : suivi des trous de caisse, éditeurs de bordereaux et archivage.
- **Administration & Paramétrage** : gestion des utilisateurs, vols, PNC, catalogues d'articles et taux de change.

---

## 🛠️ Technologies Utilisées

- **Framework** : ASP.NET Core 8.0 (MVC)
- **Langage** : C# 12
- **ORM** : Entity Framework Core 8.0
- **Base de Données** : Microsoft SQL Server
- **Authentification** : ASP.NET Core Identity (Rôles : `Admin`, `Agent`, `Catering`, `DCF`)
- **Frontend** : HTML5, CSS3, JavaScript (AJAX), Bootstrap 5, FontAwesome

---

## 🚀 Guide d'Installation et Exécution

### 1. Prérequis
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (avec le chargeur de travail *Développement Web et ASP.NET*)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (ou SQL Server Express / LocalDB)

### 2. Cloner le projet
```bash
git clone https://github.com/khadraouihiba02/Application-de-vente.git
```

### 3. Configurer la base de données
1. Ouvrez la solution `Application de vente.sln` dans **Visual Studio 2022**.
2. Ouvrez le fichier `appsettings.json`.
3. Ajustez la chaîne de connexion `DefaultConnection` avec le nom de votre serveur SQL local :
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=VOTRE_SERVEUR_SQL;Database=ApplicationDeVenteDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

### 4. Appliquer les migrations Entity Framework
Ouvrez la **Console du gestionnaire de packages** dans Visual Studio et exécutez :
```powershell
Update-Database
```
*Ou via le CLI .NET dans le terminal :*
```bash
dotnet ef database update
```

### 5. Lancer l'application
Appuyez sur **F5** (ou `dotnet run`) pour exécuter l'application.

---

## 🔐 Comptes de Démonstration

| Rôle | Adresse Email | Mot de Passe |
| :--- | :--- | :--- |
| **Admin** | `admin@tunisair.com.tn` | *(Défini lors de l'initialisation)* |
| **Catering** | `catering@tunisair.com.tn` | *(Défini lors de l'initialisation)* |
| **Agent** | `agent@tunisair.com.tn` | *(Défini lors de l'initialisation)* |
| **DCF** | `dcf@tunisair.com.tn` | *(Défini lors de l'initialisation)* |

---

## 📄 Licence & Droits
Projet développé dans le cadre de la gestion des Ventes à Bord — TUNISAIR.
