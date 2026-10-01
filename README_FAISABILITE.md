# README de faisabilite
## Migration WinForms vers Web .NET avec DDD + Clean Architecture

Ce document resume la decision de faisabilite pour la solution actuelle.

## Verdict rapide
- Faisable: Oui
- Nature du chantier: refonte progressive, pas portage direct
- Effort estime: 18 a 29 semaines
- Risques principaux: securite SQL/auth, reprise reporting, regles metier implicites

## Pourquoi c est faisable
- Domaine metier bien identifiable (tresorerie, droits, parametrage, reporting)
- Code existant exploitable comme source de regles metier
- Base SQL existante reutilisable avec couche anti-corruption

## Pourquoi ce n est pas un simple lift-and-shift
- Couplage fort UI WinForms + SQL inline + logique metier
- Bibliotheques desktop non transposables telles quelles en web
- Credentials et requetes sensibles a securiser avant exposition web

## Architecture cible proposee
- Domain: regles metier
- Application: cas d usage
- Infrastructure: acces donnees et integrations
- Web: API + UI

## Plan de migration recommande
1. Cadrage et cartographie (2-3 semaines)
2. Socle securite + auth + observabilite (4-6 semaines)
3. Flux coeur tresorerie (6-8 semaines)
4. Parametrages (4-6 semaines)
5. Reporting et exports (4-6 semaines)
6. Bascule et retrait progressif WinForms (2-4 semaines)

## Livrable detaille
Consulter le rapport complet:
- RAPPORT_FAISABILITE_MIGRATION_DDD_CLEAN.md

## Sortie use cases et architecture
- USE_CASES_ET_ARCHITECTURE_DDD_CLEAN.md

## Prerequis de lancement
- Acces complet schema SQL et vues
- Validation metier hebdomadaire
- Arbitrage outillage reporting web
- Equipe dediee (architecture, dev, QA)

## Configuration securisee
Les chaines de connexion ne doivent pas etre versionnees. Pour le developpement local, les definir avec le gestionnaire de secrets .NET depuis le dossier `CNSS_TRESO` :

```powershell
dotnet user-secrets set "ConnectionStrings:CnssTreso" "<chaine SQL chiffree>"
dotnet user-secrets set "ConnectionStrings:Budget" "<chaine SQL chiffree>"
dotnet user-secrets set "ConnectionStrings:SuiviTp" "<chaine SQL chiffree>"
dotnet user-secrets set "ConnectionStrings:Compta" "<chaine SQL chiffree>"
```

En production, definir les memes valeurs via le gestionnaire de secrets de la plateforme ou les variables d'environnement `ConnectionStrings__CnssTreso`, `ConnectionStrings__Budget`, `ConnectionStrings__SuiviTp` et `ConnectionStrings__Compta`. Les comptes SQL doivent avoir le moindre privilege necessaire et utiliser `Encrypt=True` avec un certificat valide.

## Recommandation
Demarrer par une V1 web ciblee sur:
- Authentification et droits
- Saisie et consultation des mouvements
- Rapports essentiels

Puis etendre par increment jusqu a extinction du client WinForms.
