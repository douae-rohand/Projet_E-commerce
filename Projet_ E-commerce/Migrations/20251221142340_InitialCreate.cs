using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Projet__E_commerce.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    idC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.idC);
                    table.CheckConstraint("CK_Categorie_Nom", "nom IN ('cosmetique', 'nutritive', 'medical', 'artisanat', 'agricole')");
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    est_actif = table.Column<bool>(type: "bit", nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    nom_cooperative = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    localisation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ville = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    logo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    telephone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.id);
                    table.ForeignKey(
                        name: "FK_Admins_Utilisateurs_id",
                        column: x => x.id,
                        principalTable: "Utilisateurs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    nom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    telephone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    date_naissance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_Clients_Utilisateurs_id",
                        column: x => x.id,
                        principalTable: "Utilisateurs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdmins",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdmins", x => x.id);
                    table.ForeignKey(
                        name: "FK_SuperAdmins_Utilisateurs_id",
                        column: x => x.id,
                        principalTable: "Utilisateurs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    idP = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nomP = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seuil_alerte = table.Column<int>(type: "int", nullable: false),
                    statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    idC = table.Column<int>(type: "int", nullable: false),
                    idAdmin = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.idP);
                    table.CheckConstraint("CK_Produit_Statut", "statut IN ('active', 'inactive')");
                    table.ForeignKey(
                        name: "FK_Produits_Admins_idAdmin",
                        column: x => x.idAdmin,
                        principalTable: "Admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Produits_Categories_idC",
                        column: x => x.idC,
                        principalTable: "Categories",
                        principalColumn: "idC",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdressesLivraison",
                columns: table => new
                {
                    idAdresse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idClient = table.Column<int>(type: "int", nullable: false),
                    nom_adresse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    adresse_complete = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ville = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code_postal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    telephone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    est_par_defaut = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdressesLivraison", x => x.idAdresse);
                    table.ForeignKey(
                        name: "FK_AdressesLivraison_Clients_idClient",
                        column: x => x.idClient,
                        principalTable: "Clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commandes",
                columns: table => new
                {
                    idCommande = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idClient = table.Column<int>(type: "int", nullable: false),
                    statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    prixTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandes", x => x.idCommande);
                    table.CheckConstraint("CK_Commande_Statut", "statut IN ('en_attente', 'valide', 'annule')");
                    table.ForeignKey(
                        name: "FK_Commandes_Clients_idClient",
                        column: x => x.idClient,
                        principalTable: "Clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Avis",
                columns: table => new
                {
                    idAvis = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idClient = table.Column<int>(type: "int", nullable: false),
                    idProduit = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<int>(type: "int", nullable: true),
                    commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avis", x => x.idAvis);
                    table.CheckConstraint("CK_Avis_Note", "note BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Avis_Clients_idClient",
                        column: x => x.idClient,
                        principalTable: "Clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Avis_Produits_idProduit",
                        column: x => x.idProduit,
                        principalTable: "Produits",
                        principalColumn: "idP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Variantes",
                columns: table => new
                {
                    idV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idP = table.Column<int>(type: "int", nullable: false),
                    prix = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    taille = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    couleur = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    photo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    quantite = table.Column<int>(type: "int", nullable: false),
                    poids = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variantes", x => x.idV);
                    table.ForeignKey(
                        name: "FK_Variantes_Produits_idP",
                        column: x => x.idP,
                        principalTable: "Produits",
                        principalColumn: "idP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    idFacture = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idCommande = table.Column<int>(type: "int", nullable: false),
                    numero_facture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    path_facture = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.idFacture);
                    table.ForeignKey(
                        name: "FK_Factures_Commandes_idCommande",
                        column: x => x.idCommande,
                        principalTable: "Commandes",
                        principalColumn: "idCommande",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Livraisons",
                columns: table => new
                {
                    idLivraison = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idCommande = table.Column<int>(type: "int", nullable: false),
                    idAdresse = table.Column<int>(type: "int", nullable: false),
                    statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    dateDebutEstimation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dateFinEstimation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    frais = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    mode_livraison = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livraisons", x => x.idLivraison);
                    table.CheckConstraint("CK_Livraison_ModeLivraison", "mode_livraison IN ('Standard', 'Express')");
                    table.CheckConstraint("CK_Livraison_Statut", "statut IN ('livre', 'non_livre', 'en_cours')");
                    table.ForeignKey(
                        name: "FK_Livraisons_AdressesLivraison_idAdresse",
                        column: x => x.idAdresse,
                        principalTable: "AdressesLivraison",
                        principalColumn: "idAdresse",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Livraisons_Commandes_idCommande",
                        column: x => x.idCommande,
                        principalTable: "Commandes",
                        principalColumn: "idCommande",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesCommande",
                columns: table => new
                {
                    idLC = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idCommande = table.Column<int>(type: "int", nullable: false),
                    idV = table.Column<int>(type: "int", nullable: false),
                    quantite = table.Column<int>(type: "int", nullable: false),
                    prix_unitaire = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesCommande", x => x.idLC);
                    table.ForeignKey(
                        name: "FK_LignesCommande_Commandes_idCommande",
                        column: x => x.idCommande,
                        principalTable: "Commandes",
                        principalColumn: "idCommande",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesCommande_Variantes_idV",
                        column: x => x.idV,
                        principalTable: "Variantes",
                        principalColumn: "idV",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BordereauxLivraison",
                columns: table => new
                {
                    idBordereau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idLivraison = table.Column<int>(type: "int", nullable: false),
                    numero_bordereau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    date_generation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    path_bordereau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BordereauxLivraison", x => x.idBordereau);
                    table.ForeignKey(
                        name: "FK_BordereauxLivraison_Livraisons_idLivraison",
                        column: x => x.idLivraison,
                        principalTable: "Livraisons",
                        principalColumn: "idLivraison",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "idC", "created_at", "description", "nom", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8631), "Produits cosmétiques et de beauté", "cosmetique", new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8695) },
                    { 2, new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8701), "Produits nutritionnels et alimentaires", "nutritive", new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8703) },
                    { 3, new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8706), "Produits médicaux et de santé", "medical", new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8709) },
                    { 4, new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8712), "Produits artisanaux locaux", "artisanat", new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8714) },
                    { 5, new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8717), "Produits agricoles et fermiers", "agricole", new DateTime(2025, 12, 21, 15, 23, 39, 780, DateTimeKind.Local).AddTicks(8719) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdressesLivraison_idClient",
                table: "AdressesLivraison",
                column: "idClient");

            migrationBuilder.CreateIndex(
                name: "IX_Avis_idClient",
                table: "Avis",
                column: "idClient");

            migrationBuilder.CreateIndex(
                name: "IX_Avis_idProduit",
                table: "Avis",
                column: "idProduit");

            migrationBuilder.CreateIndex(
                name: "IX_BordereauxLivraison_idLivraison",
                table: "BordereauxLivraison",
                column: "idLivraison",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_idClient",
                table: "Commandes",
                column: "idClient");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_idCommande",
                table: "Factures",
                column: "idCommande",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommande_idCommande",
                table: "LignesCommande",
                column: "idCommande");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommande_idV",
                table: "LignesCommande",
                column: "idV");

            migrationBuilder.CreateIndex(
                name: "IX_Livraisons_idAdresse",
                table: "Livraisons",
                column: "idAdresse");

            migrationBuilder.CreateIndex(
                name: "IX_Livraisons_idCommande",
                table: "Livraisons",
                column: "idCommande",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produits_idAdmin",
                table: "Produits",
                column: "idAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Produits_idC",
                table: "Produits",
                column: "idC");

            migrationBuilder.CreateIndex(
                name: "IX_Variantes_idP",
                table: "Variantes",
                column: "idP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avis");

            migrationBuilder.DropTable(
                name: "BordereauxLivraison");

            migrationBuilder.DropTable(
                name: "Factures");

            migrationBuilder.DropTable(
                name: "LignesCommande");

            migrationBuilder.DropTable(
                name: "SuperAdmins");

            migrationBuilder.DropTable(
                name: "Livraisons");

            migrationBuilder.DropTable(
                name: "Variantes");

            migrationBuilder.DropTable(
                name: "AdressesLivraison");

            migrationBuilder.DropTable(
                name: "Commandes");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
