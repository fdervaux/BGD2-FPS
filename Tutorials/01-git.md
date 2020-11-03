# Utiliser Git avec Unity

# Table of Contents
1. [Introduction](#Introduction)
2. [Git-Setup](#Git-Setup)
3. [Unity Project Setup](#unity-Project-Setup)

## Introduction

Pour gérer les différentes versions d'un projet et ne jamais perdre son travail, le plus simple est d'utiliser Git.

### Historique

Git est un logiciel de gestion de versions décentralisé. C'est un logiciel libre créé par Linus Torvalds, auteur du noyau Linux, et distribué selon les termes de la licence publique générale GNU version 2. En 2016, il s’agit du logiciel de gestion de versions le plus populaire qui est utilisé par plus de douze millions de personnes 
(source wikipedia)

## Git Setup

### Install Git For Windows

#### Description
Git for Windows se concentre sur l'offre d'un ensemble d'outils légers et natifs qui apportent l'ensemble des fonctionnalités de Git à Windows tout en fournissant des interfaces utilisateur appropriées pour les utilisateurs expérimentés de Git comme pour les novices.

Git BASH
Git pour Windows fournit une émulation BASH utilisée pour exécuter Git à partir de la ligne de commande. *Les utilisateurs de UNIX devraient se sentir comme chez eux, car l'émulation BASH se comporte exactement comme la commande "git" dans les environnements LINUX et UNIX.

Interface graphique de Git
Comme les utilisateurs de Windows s'attendent généralement à des interfaces graphiques, Git for Windows fournit également l'interface graphique Git, une puissante alternative à Git BASH, offrant une version graphique de presque toutes les fonctions de la ligne de commande Git, ainsi que des outils de comparaison visuelle complets.

Intégration du shell
Il suffit de cliquer avec le bouton droit de la souris sur un dossier dans l'explorateur Windows pour accéder à la BASH ou à l'interface graphique.

#### install
Installer git via:
https://gitforwindows.org/

### install Git-LFS

#### description
Git Large File Storage (LFS) remplace les fichiers volumineux tels que les fichiers audio, les vidéos, les ensembles de données et les images par des pointeurs de texte dans Git, tout en stockant le contenu des fichiers sur un serveur distant comme GitHub.com ou GitHub Enterprise.

#### install

- Installer Git LFS via:
  https://git-lfs.github.com/
- taper la ligne de commande suivante dans git bash
``` git lfs install ```

## Unity Project Setup