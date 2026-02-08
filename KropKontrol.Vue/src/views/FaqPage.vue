<template>
  <DefaultLayout :hideSidebar="true" showBack @back="goBack" @logout="logout">
    <template #title>Aide et FAQ</template>
    <div class="container py-4">
      <h2 class="faq-title"><center>Gestion des Utilisateurs</center></h2>
      <section>
        <p class="small text-muted">Pour les consommations d'eau, voir la section "Consommations d'eau" ci-dessus.</p>
        <h3>Changer le login et le mot de passe</h3>
        <ol>
          <li>Allez dans <em>Konfiguration &gt; Utilisateurs</em>.</li>
          <li>Choisissez votre compte et ouvrez les paramètres.</li>
          <li>Modifiez le champ désiré puis validez.</li>
        </ol>
      </section>
      <h2 class="faq-title"><center>Création de Graphiques</center></h2>
      <section>
        <h3>Créer un graphique</h3>
        <p>
          Pour créer un graphique, choisissez le Kapteur et cliquez sur le
          <strong>+</strong> dans la barre de Kapteurs. Sélectionnez les
          variables à afficher. Cliquez ensuite sur
          <strong>Actualiser</strong> pour charger les données.
        </p>
      </section>

      <h2 class="faq-title"><center>Consommations d'eau</center></h2>
      <section>
        <h3>Calcul base sur la VWC selectionnee</h3>
        <p>
          Les consommations du tableau <em>Resume des irrigations detectees</em> sont calculees a partir de la variable <strong>VWC</strong> que vous cochez dans le graphique du Kapteur.
          Si aucune VWC n'est selectionnee, ces valeurs ne sont pas affichees.
        </p>
        <ul>
          <li><strong>Conso post-arrosage (J)</strong> : VWC entre 30 min apres le dernier arrosage de J et le coucher du soleil de J.</li>
          <li><strong>Conso de nuit (J)</strong> : VWC entre le coucher du soleil de J-1 et le lever du soleil de J (affichee dans la colonne J).</li>
          <li><strong>Conso pre-arrosage (J)</strong> : VWC entre le lever du soleil de J et 5 min avant le premier arrosage de J.</li>
        </ul>
        <p class="small text-muted">
          Les heures de lever/coucher dependent de la localisation et du fuseau configures. Les valeurs utilisent les points de mesure les plus proches; de legers ecarts peuvent apparaitre selon la frequence d'echantillonnage.
        </p>

        <h3>Relation entre les 3 consommations (J)</h3>
        <p>
          Pour un jour <strong>J</strong>, trois periodes se suivent et sont liees : <em>post (J-1)</em> -> <em>nuit (J)</em> -> <em>pre (J)</em>. Ensemble, elles couvrent la fin de journee de J-1 jusqu'au premier arrosage de J.
        </p>
      </section>

      <section>
        <h3>Actions disponibles sur le graphique</h3>
        <ul class="list-unstyled">
          <li>
            📥 <strong>Export CSV</strong> : télécharge les données affichées
            actuellement à l'écran.
          </li>
          <li>
            📝 <strong>Dupliquer</strong> : crée une copie du graphique
            modifiable immédiatement en dessous du graphique existant.
          </li>
          <li>
            🖥️ <strong>Plein écran</strong> : agrandit le graphique pour une
            meilleure visibilité.
          </li>
          <li>
            🗑️ <strong>Supprimer</strong> : retire le graphique du tableau de
            bord.
          </li>
        </ul>
      </section>

      <section>
        <h3>Barre d’outils (colonne gauche)</h3>
        <ul class="list-unstyled">
          <li>🔧 <strong>Variables</strong> : cochez les séries à afficher.</li>
          <li>
            📅 <strong>Calendrier</strong> : définissez une période
            personnalisée (début/fin).
          </li>
          <li>
            📊 <strong>Statistiques</strong> : affiche min/max/moyenne
            journalière.
          </li>
          <li>
            ⚖️ <strong>Échelles</strong> : ajustez manuellement les axes (min /
            max) par variables, les valeurs valeurs renseignées sont stockées
            sur le navigateur.
          </li>
          <li>
            ☀️ <strong>Soleil</strong> : appliquez un décalage aux annotations
            lever/coucher du soleil.
          </li>
          <li>
            <strong>Boutons d’intervalle</strong> : sélection rapide d’une
            période (36h jour, 3 jours, etc.).
          </li>
          <li>
            🔄 <strong>Actualiser</strong> : recharge le graphique avec les
            paramètres choisis.
          </li>
          <li>
            L'overview en dessous du grapique principal affiche les 30 derniers
            jours. La zone grisée correspond à la période affichées sur le
            graphique principale. Vous pouvez la déplacer et la redimensionner.
          </li>
        </ul>
      </section>

      <h2 class="faq-title"><center>Gestion des Kapteurs</center></h2>
      <section>
        <h3>Modifier les propriétés d’un Kapteur</h3>
        <ol>
          <li>
            <strong>Ouvrir la page de configuration</strong>
            <ol>
              <li>
                Depuis l’écran « Gestion des utilisateurs et Kapteurs », cliquez
                sur <em>Configuration spécifique des Kapteurs</em>.
              </li>
              <li>
                La page affiche la liste des Kapteurs dans la barre latérale ;
                sélectionnez celui à modifier.
              </li>
            </ol>
          </li>
          <li>
            <strong>Renommer le Kapteur (champ Description)</strong>
            <ol>
              <li>
                Dans la section <em>Configuration du Kapteur</em>, repérez le
                champ <em>Description</em>.
              </li>
              <li>
                Remplacez le texte existant par le nouveau nom souhaité, puis
                cliquez sur <strong>Enregistrer</strong> pour appliquer les
                changements.
              </li>
            </ol>
          </li>
          <li>
            <strong>Changer la localisation</strong>
            <ol>
              <li>
                Toujours dans <em>Configuration du Kapteur</em>, renseignez la
                zone <em>Localisation du Kapteur</em> avec la nouvelle position
                ou description du lieu.
              </li>
              <li>
                Validez à nouveau avec <strong>Enregistrer</strong> pour
                sauvegarder la modification.
              </li>
            </ol>
          </li>
          <li>
            <strong>Configurer des alertes de seuils</strong>
            <ol>
              <li>Dans la section <em>Définir des seuils d’alertes</em> :</li>
              <li>
                Choisissez la <em>Variable</em> à surveiller dans la liste
                déroulante.
              </li>
              <li>
                Remplissez, si nécessaire, les champs <em>Seuil bas</em>,
                <em>Seuil haut</em> et <em>Hystérésis</em> (la marge utilisée
                pour lever l’alerte).
              </li>
              <li>
                Cliquez sur
                <strong>Valider les paramètres de seuils</strong> pour
                enregistrer l’alerte.
              </li>
              <li>
                Les seuils existants apparaissent dans la liste
                <em>Seuils définis</em> ; utilisez
                <strong>Supprimer</strong> pour retirer un seuil.
              </li>
            </ol>
          </li>
        </ol>
      </section>

      <section>
        <h3>Associer plusieurs Kapteurs</h3>
        <ol>
          <li>Ouvrez la page <em>Kapteurs &gt; Association</em>.</li>
          <li>Donnez un nom au groupe de Kapteurs.</li>
          <li>
            Dans la liste des Kapteurs ci-dessous, sélectionnez ceux que vous
            souhaitez associer puis cliquez sur « Ajouter au groupe ».
          </li>
          <li>
            Le groupe de Kapteurs sera dorénavant disponible dans la sidebar
            Kapteurs et vous pourrez générer un graphique spécifique.
          </li>
          <li>
            Pour supprimer un groupe de Kapteurs, sélectionnez-le dans la
            sidebar et cliquez sur « Supprimer ».
          </li>
        </ol>
      </section>

      <h2 class="faq-title">
        <center>Options avancées dans KropKontrol IoT Dashboard</center>
      </h2>
      <section>
        <h3>Créer des Graphiks personnalisés</h3>
        <ol>
          <li>
            Ouvrez la page <em>Graphiks personnalisés</em> depuis l’accueil.
          </li>
          <li>
            Cliquez sur le bouton <strong>+</strong> en haut de la barre
            latérale.
          </li>
          <li>
            Sélectionnez le <em>Kapteur</em> et la <em>Variable</em>, puis
            cliquez sur <strong>Ajouter la courbe</strong>.
          </li>
          <li>
            Répétez pour chaque courbe, puis validez avec
            <strong>Ajouter ce graphique</strong>.
          </li>
        </ol>
      </section>

      <section>
        <h3>Générer un Rapport Klimat</h3>
        <ol>
          <li>
            Depuis la page KropKontrol IoT Dashboard, cliquez sur
            <em>Rapport Klimat</em>.
          </li>
          <li>
            Choisissez le Kapteur et le rapport des 7 derniers jours sera
            généré.
          </li>
        </ol>
      </section>

      <section>
        <h3>Consulter le Rapport d’arrosage</h3>
        <ol>
          <li>Ouvrez <em>Rapports &gt; Rapport d’arrosage</em>.</li>
          <li>
            Choisissez le Kapteur et le rapport des 7 derniers jours sera
            généré.
          </li>
        </ol>
      </section>

      <h2 class="faq-title"><center>Données</center></h2>
      <section>
        <h3>Réinitialiser les données locales (IndexedDB)</h3>
        <p>
          Utilisez ce bouton pour effacer toutes les données mises en cache sur
          votre ordinateur.
        </p>
        <div class="d-flex align-items-center gap-3 flex-wrap">
          <div class="form-check">
            <input class="form-check-input" type="checkbox" id="onlyAccessible"
                   v-model="onlyAccessible">
            <label class="form-check-label" for="onlyAccessible">
              Limiter aux Kapteurs visibles (accessibles)
            </label>
          </div>
          <button class="btn btn-danger" @click="handleReset">
            Effacer la base locale
          </button>
          <button class="btn btn-secondary" @click="handleBackfill30">
            Remplir le cache (30j)
          </button>
        </div>
      </section>

      <h2 class="faq-title"><center>Définitions</center></h2>
      <section>
        <h3>Consommation d'eau post-arrosage (J)</h3>
        <p>
          Différence de permittivité mesurée entre 30 minutes après le dernier
          arrosage détecté de la journée et le coucher du soleil. La différence
          calculée peu légèrement différer de la valeur calculée à partir du VWC
        </p>

        <h3>Consommation d'eau de nuit</h3>
        <p>
          Différence de permittivité mesurée entre le coucher du soleil et le
          lever du soleil (lendemain). La différence calculée peu légèrement
          différer de la valeur calculée à partir du VWC
        </p>

        <h3>Consommation d'eau pré arrosage (J)</h3>
        <p>
          Différence de permittivité mesurée entre le lever du soleil et 5
          minutes avant le premier arrosage détecté de la journée. La différence
          calculée peu légèrement différer de la valeur calculée à partir du VWC
        </p>

        <h3>VPD 3-8 (h)</h3>
        <p>
          Nombre d'heures durant lesquelles le déficit de pression de vapeur est
          compris entre 3 et 8 g/m3.
        </p>

        <h3>Rosée &lt; T (h)</h3>
        <p>
          Durée pendant laquelle la température de l'air reste supérieure au
          point de rosée, signalant l'absence de condensation. si la valeur est
          inférieure à 24h, il peut y avoir des risques de développement de
          maladie.
        </p>

        <h3>Humidité absolue</h3>
        <p>
          Masse de vapeur d'eau contenue dans un mètre cube d'air
          (g/m<sup>3</sup>).
        </p>

        <h3>VPD</h3>
        <p>
          Déficit de pression de vapeur : différence entre la pression de vapeur
          saturante et la pression de vapeur réelle.
        </p>
      </section>
      <h2 class="faq-title"><center>Questions Fréquentes</center></h2>
      <section>
        <h3>A quelle fréquence sont collectées les données?</h3>
        <p>
          Les données sont collectées en temps réel, avec un intervalle de
          variable suivant le type de culture et l'objectif. Réduire ou
          augmenter la fréquence, impacte la durée de vie de la batterie de vos
          emetteurs.
        </p>
        <p>
          Les données sont stockées localement dans votre navigateur pour un
          accès rapide et une meilleure performance.
        </p>
        <h3>
          S'il y a une coupure d'électricité et ou d'internet qu'advient-t'il de
          mes données?
        </h3>
        <p>
          Pour l'heure, nous ne pouvons récupérer les données absentes suite à
          une coupure d'électricité ou d'internet
        </p>
        <p>
          Les données sont stockées localement dans votre navigateur pour un
          accès rapide et une meilleure performance.
        </p>
        <h3>
          Je dois changer la batterie d'un de mes capteurs qu'elle est sa
          référence?
        </h3>
        <p>
          <strong>Milesight UC502</strong> : 3 × ER26500 Li-SOCl₂
          (lithium-thionyl chloride) 9000 mAh
        </p>
        <p>
          <strong>Milesight EM300-TH ou DI</strong> : 1 × ER18505 Li-SOCl₂ 4000
          mAh par défaut (emplacement double possible pour 8000 mAh au total)
        </p>
      </section>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { useRouter } from "vue-router";
import { useAuth } from "@/composables/useAuth.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { clearCache, ensureCacheForLastNDays } from "@/services/dataCacheService.js";
import { useDevices } from "@/composables/useDevices.js";
import { useVirtualDevices } from "@/composables/useVirtualDevices.js";
import { ref } from 'vue'

const router = useRouter();
const { logout } = useAuth();
const { devices: fetchedDevices, loadDevices } = useDevices();
const { virtualDevices } = useVirtualDevices();
const onlyAccessible = ref(true)

function goBack() {
  router.back();
}

async function handleReset() {
  try {
    await clearCache();
    window.alert("Cache local supprimé");
    window.location.reload(true);
  } catch (err) {
    console.error(err);
    window.alert("Échec de la suppression du cache local");
  }
}

async function handleBackfill30() {
  try {
    await loadDevices();
    // Pour le moment, la liste retournée par useDevices/useVirtualDevices
    // correspond déjà aux Kapteurs accessibles à l'utilisateur.
    // L'option "onlyAccessible" garde la porte ouverte pour une future extension
    // (ex: administrateur pouvant inclure des Kapteurs non visibles).
    const list = [...(fetchedDevices.value ?? []), ...(virtualDevices.value ?? [])];
    const tasks = [];
    for (const dev of list) {
      if (!dev?.devEui) continue;
      if (dev.isVirtual && dev.group?.deviceModels) {
        const dm = dev.group.deviceModels || {};
        if (onlyAccessible.value) {
          // Restreint strictement aux membres visibles du groupe
          for (const [de, m] of Object.entries(dm)) {
            if (de && m != null) tasks.push(ensureCacheForLastNDays(de, Number(m), 30));
          }
        } else {
          // À étendre: inclure d'autres Kapteurs non visibles si une API le permet
          for (const [de, m] of Object.entries(dm)) {
            if (de && m != null) tasks.push(ensureCacheForLastNDays(de, Number(m), 30));
          }
        }
      } else if (dev.model != null) {
        tasks.push(ensureCacheForLastNDays(dev.devEui, Number(dev.model), 30));
      }
    }
    const res = await Promise.allSettled(tasks);
    const ok = res.filter(r => r.status === 'fulfilled').length;
    window.alert(`Backfill terminé (30j). Tâches OK: ${ok}/${res.length}`);
  } catch (err) {
    console.error(err);
    window.alert("Échec du backfill 30j");
  }
}
</script>

<style scoped>
:root {
} /* rien ici, mais garde si tu veux centraliser les couleurs */

.faq-title {
  /* layout */
  position: relative;
  margin: 2.5rem 0 1rem;
  padding: 0.75rem 1rem 0.75rem 1.25rem;

  /* typo */
  font-size: clamp(1.6rem, 2.2vw, 2rem);
  font-weight: 800;
  line-height: 1.2;
  letter-spacing: 0.02em;

  /* couleurs (ajuste la couleur d'accent si besoin) */
  --faq-accent: #8dc63f; /* vert KropKontrol ? adapte au besoin */
  background: linear-gradient(
    90deg,
    rgba(141, 198, 63, 0.1),
    rgba(141, 198, 63, 0.06) 40%,
    transparent 100%
  );
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-left: 8px solid var(--faq-accent);
  border-radius: 12px;

  /* relief */
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

/* petite ligne décorative sous le h2 pour accentuer la séparation */
.faq-title::after {
  content: "";
  display: block;
  height: 3px;
  margin-top: 8px;
  border-radius: 2px;
  background: linear-gradient(90deg, var(--faq-accent), transparent);
  opacity: 0.5;
}

/* option : rendre les h2 “sticky” au scroll (dé-commente si tu veux) */
/*
.faq-title {
  position: sticky;
  top: 60px;              // ajuste selon la hauteur de ton header
  z-index: 1;
  backdrop-filter: blur(2px);
}
*/

/* sous-titres h3 : lisibles mais moins “lourds” que h2 */
h3 {
  margin: 1.25rem 0 0.5rem;
  font-weight: 700;
  border-bottom: 1px solid #e5e7eb;
  padding-bottom: 0.25rem;
}

/* espacement vertical propre entre sections */
section + section {
  margin-top: 1.25rem;
}
.list-unstyled {
  list-style: none;
  padding-left: 0;
}
/* ligne de séparation douce sur toute la largeur si nécessaire (optionnel) */
/*
hr.faq-sep {
  border: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(0,0,0,.12), transparent);
  margin: 2rem 0;
}
*/
</style>
