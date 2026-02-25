# Projet E-commerce 🛍️

<div align="center">
  <h1 style="font-family: 'Playfair Display', serif; color: #D87A4F; font-size: 3em;">Souk 212 Platform</h1>
  <p style="font-size: 1.2em; color: #ffffffff;">La solution digitale premium pour l'essor des coopératives marocaines</p>
  
  <p>
    <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET" />
    <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet" alt="ASP.NET Core" />
    <img src="https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoft-sql-server" alt="SQL Server" />
    <img src="https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap" alt="Bootstrap" />
    <img src="https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=for-the-badge&logo=.net" alt="EF Core" />
  </p>
</div>


A modern, responsive e-commerce platform built with **ASP.NET Core 8.0 MVC**. This project aims to provide a seamless shopping experience with a focus on ease of use and modern design.

##  Features

- **Storefront**: Browse products with a clean and intuitive interface.
- **AI Assistant**: Integrated AI-powered chatbot (via n8n and Gemini) to help users find products and answer questions.
- **Product Management**: Robust backend for managing categories and products.
- **Shopping Cart**: Fully functional cart system for a smooth checkout process.
- **Responsive Design**: Built with Bootstrap to ensure it looks great on all devices.

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


## 🛠️ Technology Stack

- **Framework**: [ASP.NET Core 8.0 MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview)
- **Frontend**: HTML5, CSS3, JavaScript, [Bootstrap 5](https://getbootstrap.com/)
- **Runtime**: .NET 8.0
- **Development Tool**: Visual Studio 2022

##  Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the "ASP.NET and web development" workload.

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/douae-rohand/Projet_E-commerce.git
   cd Projet_E-commerce
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the application**:
   ```bash
   dotnet run --project "Projet_ E-commerce"
   ```
   Or simply open the `.sln` file in Visual Studio and press `F5`.

## 📂 Project Structure

- `Projet_ E-commerce/`: Main project directory.
  - `Controllers/`: Handles incoming requests and orchestrates views.
  - `Models/`: Data structures and business logic.
  - `Views/`: Razor components for the UI.
  - `wwwroot/`: Static assets (CSS, JS, Images, Libraries).


## 📄 License
This project is open-sourced software licensed under the [MIT license](https://opensource.org/licenses/MIT).
