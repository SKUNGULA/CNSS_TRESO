# Rapport complet de faisabilite
## Migration de la solution WinForms vers une application Web .NET (DDD + Clean Architecture)

Date: 2026-05-29
Perimetre analyse: solution DRH17.sln (projets DRH2018 et dgio_metiers)

## 1. Resume executif
La migration est faisable, mais elle releve d une refonte structurelle et pas d un simple portage ecran par ecran.

Verdict:
- Faisabilite technique: Oui
- Type de migration recommande: progressive, par domaine fonctionnel, avec coexistence temporaire WinForms/Web
- Niveau d effort: eleve (refonte architecture, securite, acces donnees, reporting)
- Benefice attendu: tres eleve (maintenabilite, securite, scalabilite, auditabilite)

Conclusion breve:
- Le code metier est fortement couple a l infrastructure SQL et a l UI.
- Une migration directe vers DDD + Clean Architecture est possible en 4 a 6 mois avec une equipe dediee.
- Le meilleur chemin est un modele de transition en tranches fonctionnelles, en priorisant authentification, saisie des mouvements, consultation et rapports.

## 2. Donnees factuelles relevees
Metrie code (estimation sur fichiers C#):
- Fichiers C#: 38
- Fichiers Designer WinForms: 9
- Definitions de classes detectees: 30
- Occurrences d acces SQL direct detectees (SqlCommand/SqlConnection/Execute*): 269
- Plus gros hotspots:
  - DRH2018/Formulaires/frmsaisiecb.designer.cs: 16819 lignes
  - DRH2018/Formulaires/preparer.designer.cs: 3072 lignes
  - DRH2018/Formulaires/preparer.cs: 974 lignes
  - DRH2018/Formulaires/frmsaisiecb.cs: 951 lignes
  - dgio_metiers/Chargement.cs: 931 lignes
  - dgio_metiers/FondsEtats.cs: 848 lignes

Ces metriques confirment un couplage fort UI + logique metier + SQL, typique d un legacy WinForms.

## 3. Etat actuel de l architecture
### 3.1 Structure observee
- Projet presentation WinForms: DRH2018 (target .NET Framework 4.5)
- Projet metier/donnees: dgio_metiers (egalement .NET Framework 4.5)
- Dependance directe du projet WinForms vers le projet metier
- Absence de couches explicites Domain/Application/Infrastructure

### 3.2 Couplages critiques observes
1) Logique SQL inline et concatenee dans les classes metier
- dgio_metiers/Chargement.cs contient de nombreuses requetes construites par concatenation (ex: lignes 120, 207, 642, 716, 953)
- dgio_metiers/Users.cs (ligne 36, 41)
- dgio_metiers/Parametres.cs (ligne 35, 40)

2) Connexions SQL hardcodees et secrets en clair
- dgio_metiers/Connexiondao.cs: 31, 39, 47
- DRH2018/Program.cs: 67

3) UI WinForms pilote la logique metier directement
- DRH2018/Formulaires/frmConsultation.cs: 124 (ouverture menu apres auth)
- DRH2018/Formulaires/frmMenu.cs: 95, 102, 109, 123 (navigation par formulaires)
- DRH2018/Formulaires/frmsaisiecb.cs: forte logique d orchestration metier + presentation

4) Usage de bibliotheques desktop non transposables telles quelles en web
- Telerik/DevComponents/Infragistics WinForms references dans DRH2018.csproj
- ReportViewer WinForms references dans DRH2018.csproj et dgio_metiers.csproj
- Interop Excel: DRH2018/Program.cs: 15, 282-283 et dgio_metiers/Connexiondao.cs: 9, 185-193

5) Authentification a risque et mot de passe non hache
- Verification sur champs utilisateur + mot de passe en clair: dgio_metiers/Chargement.cs: 642
- Note metier explicite de besoin de chiffrement: REQUETES_RAPPORT.txt

### 3.3 Qualite et robustesse
- Multiples blocs catch (Exception ex) vides (perte d observabilite)
- Transactions SQL gerees localement sans politique globale
- Faible testabilite actuelle (absence de couche application claire, DataTable partout)
- Presence possible de code obsolete (ex: FondsEtats.cs present mais non compile dans le csproj actuel)

## 4. Faisabilite par axe
### 4.1 Faisabilite fonctionnelle
Oui. Les cas d usage identifies sont transposables en web:
- Authentification/autorisation
- Saisie mouvements caisse/banque
- Parametrage taux
- Gestion utilisateurs et droits
- Consultation, deconfirmation, et rapports

### 4.2 Faisabilite technique
Oui, avec refonte:
- Refaire les couches Domain/Application/Infrastructure
- Exposer API web (ou MVC/Razor) pour les cas d usage
- Remplacer DataTable + SQL concatene par Repositories/Query Services securises
- Remplacer ReportViewer WinForms/Interop Excel par solution reporting web

### 4.3 Faisabilite organisationnelle
Conditionnelle:
- Exige un schema de base de donnees stabilise
- Exige arbitrage sur reporting cible
- Exige disponibilite metier pour valider les invariants comptables

## 5. Risques majeurs et mitigations
### Risque 1: Regression fonctionnelle sur regles metier implicites
Cause: Regles embarquees dans formulaires et SQL inline.
Mitigation:
- Atelier de cartographie des regles par use case
- Tests d acceptation metier sur jeux de donnees reels
- Migration progressive par flux

### Risque 2: Vulnerabilites SQL Injection et securite credentials
Cause: Concatenation SQL et secrets en clair.
Mitigation:
- Parameterisation systematique
- Secret management (Azure Key Vault ou equivalent)
- Hash mot de passe (Argon2/PBKDF2/Bcrypt) + politique de rotation

### Risque 3: Reporting complexe a reproduire
Cause: Dependance WinForms ReportViewer + Excel Interop.
Mitigation:
- Standardiser sorties PDF/Excel cote serveur
- Introduire moteur de templates reports (RDLC serveur, FastReport, QuestPDF + ClosedXML selon choix)
- Valider par rapprochement chiffrant WinForms vs Web sur echantillon

### Risque 4: Couplage fort DB historique
Cause: SQL construit dynamiquement avec nom de base/exercice.
Mitigation:
- Introduire couche Query specifiquement pour requetes legacy
- Isoler multi-exercice dans service de resolution de schema

## 6. Architecture cible recommandee (DDD + Clean)
### 6.1 Decoupage en projets
- src/Domain
  - Entites, Value Objects, regles metier, evenements domaine
- src/Application
  - Use cases (Commands/Queries), DTO, validation, ports
- src/Infrastructure
  - EF Core/Dapper, repositories, providers SQL, integrations externes
- src/Web
  - ASP.NET Core (API + UI web)
- tests/Domain.Tests
- tests/Application.Tests
- tests/IntegrationTests

### 6.2 Bounded contexts proposes
- Identite et Acces
  - Utilisateur, role/niveau, droits formulaire/module
- Tresorerie Mouvements
  - Journaux, mouvements, validation/devalidation, soldes
- Parametrage
  - Taux, monnaies, periodes, documents
- Reporting
  - Rapports caisse/banque, releves, signataires, exports

### 6.3 Pattern de persistance
- Commandes metier: repositories transactionnels
- Requetes lourdes et rapports: Query services optimises (Dapper SQL parametre)
- Eviter DataTable dans le coeur: utiliser DTO explicites

## 7. Strategie de migration recommandee
Approche: Strangler + coexistence progressive

Phase 0 (2 a 3 semaines): cadrage
- Cartographie use cases, tables, vues SQL
- Definition contrats API
- Setup socle Clean Architecture .NET 8

Phase 1 (4 a 6 semaines): securite + socle
- Auth web (JWT/Cookie selon UI)
- Gestion utilisateurs/roles/droits
- Secrets externalises
- Observabilite (logs, traces, audit)

Phase 2 (6 a 8 semaines): flux coeur tresorerie
- Saisie caisse/banque
- Validation/devalidation mouvements
- Solde et controles

Phase 3 (4 a 6 semaines): parametrages
- Taux, journaux, modules, affectations

Phase 4 (4 a 6 semaines): reporting et exports
- Rapports metier equivalence fonctionnelle
- Export Excel/PDF

Phase 5 (2 a 4 semaines): bascule et decommission
- Double run WinForms/Web
- Reconciliation chiffres
- Retrait progressif WinForms

Estimation globale: 18 a 29 semaines calendaires selon equipe et disponibilite metier.

## 8. Equipe cible minimale
- 1 Architecte .NET (DDD/Clean)
- 2 Developpeurs .NET full stack
- 1 QA/Test engineer
- 1 Referent metier tresorerie (part-time)

## 9. Backlog technique prioritaire
Priorite critique:
1. Externaliser toutes les chaines de connexion et secrets
2. Supprimer SQL concatene dans les flux auth et ecritures
3. Introduire modele utilisateur securise (hash + audit)
4. Encapsuler transactions metier (unit of work)
5. Mettre des tests de non-regression sur calculs et validation

Priorite elevee:
1. Remplacer DataTable par DTO metier
2. Centraliser gestion des erreurs et logs
3. Introduire mapping anti-corruption entre schema legacy et domaine

## 10. Criteres Go/No-Go
Go si:
- Acces aux schemas SQL et vues accorde
- Arbitrage reporting valide
- Disponibilite metier pour validation hebdomadaire
- Budget equipe sur 4 a 6 mois

No-Go temporaire si:
- Aucun interlocuteur metier pour valider les ecarts
- Impossibilite de toucher auth et secrets
- Contraintes fortes imposant reproduction pixel-perfect de tous ecrans des la V1

## 11. Recommandations finales
- Ne pas faire une traduction ecran WinForms vers pages web a l identique.
- Faire une migration orientee cas d usage, avec contrat metier explicite.
- Prioriser la securisation et la fiabilite des flux financiers avant l ergonomie avancee.
- Adopter .NET 8 LTS pour la cible web.

## 12. Annexes: preuves techniques (extraits de localisation)
- DRH17.sln: 6-9 (2 projets, WinForms + metier)
- DRH2018/DRH2018.csproj: 14 (.NET 4.5), references WinForms/UI lourdes
- dgio_metiers/Connexiondao.cs: 31, 39, 47 (connexions hardcodees)
- dgio_metiers/Chargement.cs: 120, 207, 642, 716, 953 (SQL inline + auth + mouvements + reporting)
- DRH2018/Formulaires/frmConsultation.cs: 124-125 (flow login -> menu)
- DRH2018/Formulaires/frmMenu.cs: 95-199 (navigation et modules)
- DRH2018/Program.cs: 67 (chaine SQL en clair), 282-283 (Excel interop)
- REQUETES_RAPPORT.txt: demande explicite de chiffrement mot de passe

---
Decision recommandee: lancer la migration avec une phase de cadrage courte, puis execution incrémentale par domaines fonctionnels.
