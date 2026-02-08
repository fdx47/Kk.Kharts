<template>
  <q-page class="kk-page">
    <!-- Header -->
    <div class="kk-page__header">
      <h1 class="page-title">{{ $t('support.title') }}</h1>
      <p class="page-subtitle">{{ $t('support.subtitle') }}</p>
    </div>

    <!-- Support Form -->
    <div class="kk-card">
      <q-form @submit="sendMessage">
        <q-input
          v-model="message"
          type="textarea"
          :placeholder="$t('support.messagePlaceholder')"
          outlined
          autogrow
          :rules="[
            val => val.length >= 10 || $t('support.minLength'),
            val => val.length <= 5000 || $t('support.maxLength')
          ]"
          class="kk-input mb-4"
        />

        <q-btn
          type="submit"
          unelevated
          color="primary"
          :label="$t('support.send')"
          :loading="sending"
          :disable="message.length < 10"
          class="full-width kk-btn kk-btn--primary"
        />
      </q-form>
    </div>

    <!-- Contact Info -->
    <div class="kk-page__section mt-6">
      <h3 class="section-title">Autres moyens de contact</h3>
      
      <div class="kk-card contact-card" @click="openTelegram">
        <q-icon name="mdi-telegram" size="32px" color="info" />
        <div class="contact-info">
          <h4>Telegram</h4>
          <p>@KropKontrolBot</p>
        </div>
        <q-icon name="mdi-chevron-right" />
      </div>

      <div class="kk-card contact-card" @click="openEmail">
        <q-icon name="mdi-email" size="32px" color="primary" />
        <div class="contact-info">
          <h4>Email</h4>
          <p>support@kropkontrol.com</p>
        </div>
        <q-icon name="mdi-chevron-right" />
      </div>
    </div>

    <!-- FAQ -->
    <div class="kk-page__section">
      <h3 class="section-title">Questions fréquentes</h3>
      
      <q-expansion-item
        v-for="(faq, index) in faqs"
        :key="index"
        :label="faq.question"
        class="faq-item"
        expand-icon-class="text-primary"
      >
        <div class="faq-answer">{{ faq.answer }}</div>
      </q-expansion-item>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { api } from '@/services/api'
import { useSettingsStore } from '@/stores/settings'
import { Notify } from 'quasar'

const settingsStore = useSettingsStore()

const message = ref('')
const sending = ref(false)

const faqs = [
  {
    question: 'Comment ajouter un nouveau capteur ?',
    answer: 'Pour ajouter un nouveau capteur, contactez notre équipe support qui configurera le capteur pour votre compte.'
  },
  {
    question: 'Comment configurer les alertes ?',
    answer: 'Accédez à la page de détail d\'un capteur et cliquez sur "Configurer alertes" pour définir les seuils et les périodes d\'alerte.'
  },
  {
    question: 'Pourquoi mon capteur apparaît hors ligne ?',
    answer: 'Un capteur peut apparaître hors ligne si la batterie est faible, s\'il y a un problème de réseau LoRaWAN, ou si le capteur est physiquement endommagé.'
  },
  {
    question: 'Comment exporter mes données ?',
    answer: 'L\'export de données est disponible via l\'application web KropKontrol. Connectez-vous sur kropkontrol.com pour accéder à cette fonctionnalité.'
  }
]

async function sendMessage() {
  if (message.value.length < 10) return

  sending.value = true
  settingsStore.hapticFeedback('medium')

  try {
    await api.post('/miniapp/support', {
      message: message.value
    })

    Notify.create({
      message: 'Message envoyé avec succès !',
      color: 'positive',
      icon: 'mdi-check'
    })

    message.value = ''
  } catch (error) {
    Notify.create({
      message: 'Erreur lors de l\'envoi du message',
      color: 'negative',
      icon: 'mdi-alert'
    })
  } finally {
    sending.value = false
  }
}

function openTelegram() {
  settingsStore.hapticFeedback('light')
  window.open('https://t.me/KropKontrolBot', '_blank')
}

function openEmail() {
  settingsStore.hapticFeedback('light')
  window.location.href = 'mailto:support@kropkontrol.com'
}
</script>

<style lang="scss" scoped>
.contact-card {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 12px;
  cursor: pointer;

  &:active {
    transform: scale(0.98);
  }

  .contact-info {
    flex: 1;

    h4 {
      font-size: 14px;
      font-weight: 700;
      margin: 0 0 2px 0;
    }

    p {
      font-size: 12px;
      opacity: 0.6;
      margin: 0;
    }
  }
}

.faq-item {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  margin-bottom: 8px;
  overflow: hidden;

  :deep(.q-item) {
    padding: 16px;
  }

  :deep(.q-item__label) {
    font-weight: 600;
    font-size: 14px;
  }
}

.faq-answer {
  padding: 0 16px 16px;
  font-size: 14px;
  opacity: 0.8;
  line-height: 1.5;
}

.mb-4 {
  margin-bottom: 16px;
}

.mt-6 {
  margin-top: 24px;
}

.full-width {
  width: 100%;
}
</style>
