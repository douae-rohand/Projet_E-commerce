<div align="center">
  <h1 style="font-family: 'Playfair Display', serif; color: #D87A4F; font-size: 3em;">SOOK212 Platform</h1>
  <p style="font-size: 1.2em; color: #555;">La solution digitale premium pour l'essor des coopératives marocaines</p>
  
  <p>
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET" />
    <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet" alt="ASP.NET Core" />
    <img src="https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoft-sql-server" alt="SQL Server" />
    <img src="https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap" alt="Bootstrap" />
    <img src="https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=.net" alt="EF Core" />
  </p>
</div>

---

##  À Propos du Projet

**CoopMaroc** est une plateforme E-commerce multi-acteurs conçue pour digitaliser et dynamiser l'activité des coopératives locales. Elle sert de pont technologique entre le savoir-faire artisanal/agricole et le consommateur final, tout en offrant des outils de gestion puissants pour les administrateurs.

L'objectif est double :
1.  **Pour les Coopératives** : Offrir une vitrine professionnelle et des outils de gestion des stocks et ventes simplifiés.
2.  **Pour les Clients** : Proposer une expérience d'achat fluide, sécurisée et valorisante pour les produits du terroir.

---

##  Design & Expérience Utilisateur (UX/UI)

L'interface a été méticuleusement conçue pour refléter qualité et confiance, s'éloignant des standards génériques pour offrir une identité visuelle forte.

-   **Charte Graphique "Terre & Modernité"** :
    -   **Terracotta (#D87A4F)** : Rappelle la terre, l'argile et l'artisanat.
    -   **Dark Mode Sidebar** : Contraste élégant pour les interfaces d'administration.
    -   **Cards & Glassmorphism** : Utilisation d'ombres douces (`shadow-soft`) et de coins arrondis pour une sensation de modernité et de légèreté.
-   **Typography** : Association de *Playfair Display* (titres) pour le côté "prestige" et *Inter* (texte) pour une lisibilité optimale sur écran.

---

##  Fonctionnalités Détaillées par Rôle

### 1.  Super Admin (Gestionnaire de la Plateforme)
Le chef d'orchestre de la plateforme dispose d'une vue à 360°.

*   **Tableau de Bord Global (Dashboard)** :
    *   Statistiques en temps réel : Chiffre d'affaires global, nombre de commandes, utilisateurs actifs.
    *   Graphiques interactifs (Chart.js) pour la répartition des stocks et l'état des comptes.
    *   Top 5 des Coopératives et Top Produits performants.
*   **Logistique Centralisée ("Appel Livraison et Commande")** :
    *   Interface dédiée au suivi du cycle de vie des commandes.
    *   Vision claire des statuts : *En attente* ➝ *Validée* ➝ *En préparation* ➝ *En livraison* ➝ *Livré*.
    *   Gestion des modes de livraison (Standard vs Express).
*   **Contrôle Qualité & Sécurité** :
    *   Surveillance des stocks critiques (Alertes automatiques).
    *   Modération des commentaires et avis clients.

### 2.  Admin Coopérative (Vendeur)
Chaque coopérative gère son propre espace autonome.

*   **Catalogue Produits Avancé** :
    *   Système CRUD complet (Création, Lecture, Mise à jour, Suppression).
    *   **Gestion des Variantes** : Un même produit (ex: Huile d'Argan) peut avoir plusieurs variantes (taille, contenu) avec des prix et stocks distincts.
    *   Gestion des seuils d'alerte de stock par produit.
*   **Suivi des Ventes** :
    *   Accès uniquement aux commandes concernant ses propres produits.
    *   Génération de bons de livraison.

### 3. Client (Acheteur)
Une expérience E-commerce complète.

*   **Navigation Intuitive** : Recherche par catégories (*Cosmétique, Nutritive, Médical, Artisanat, Agricole*).
*   **Compte Client** : Historique des commandes, gestion des adresses de livraison multiples.
*   **Panier & Commande** : Processus de checkout optimisé.
*   **Social** : Possibilité de laisser des avis et notes sur les produits achetés.

---

##  Architecture Technique & Base de Données

Le projet repose sur une architecture robuste et extensible.

### Modèle de Données (Entity Framework Core)
La base de données est structurée autour de relations clés :

*   `Utilisateurs` (Base) ➝ Hérité par `Admins`, `Clients`, `SuperAdmins`.
*   `Produits` ➝ `Variantes` (Gestion fine des stocks/prix) ➝ `LignesCommande`.
*   `Commandes` ➝ `Livraisons` ➝ `AdressesLivraison`.
*   `Avis` : Liés aux `Clients` et `Produits`.

### Sécurité
*   **Authentification** : Gestion des sessions sécurisées.
*   **Autorisation** : Filtres d'action personnalisés `[AuthorizeRole("SUPER_ADMIN")]` pour protéger les routes sensibles.
*   **Protection CSRF** : Validation des jetons anti-falsification sur tous les formulaires.

---

##  Guide d'Installation

### Prérequis
*   Visual Studio 2022 ou VS Code.
*   .NET SDK 7.0 / 8.0.
*   SQL Server (Express ou version développeur).

### Étapes de déploiement

1.  **Cloner le dépôt**
    ```bash
    git clone https://github.com/douae-rohand/Projet_E-commerce.git
    cd Projet_E-commerce
    ```

2.  **Configuration Base de Données**
    Ouvrez `appsettings.json` et adaptez la chaîne de connexion `DefaultConnection` à votre instance SQL Server locale.

3.  **Restaurer les dépendances & Base de données**
    ```bash
    dotnet restore
    dotnet ef database update
    ```
    *Cela créera automatiquement la base de données et les tables grâce aux migrations EF Core.*

4.  **Lancer l'application**
    ```bash
    dotnet run --project "Projet_ E-commerce"
    ```
    Accédez à `https://localhost:7189` dans votre navigateur.

5.  **Comptes par défaut (Seed Data)**
    *L'application peut être configurée pour créer un compte SuperAdmin par défaut au premier lancement (consulter `Program.cs` ou les migrations).*

---

<div align="center">
  <p>Projet réalisé avec passion pour le développement Web Avancé.</p>
  <small>© 2025 CoopMaroc Team</small>
</div>
