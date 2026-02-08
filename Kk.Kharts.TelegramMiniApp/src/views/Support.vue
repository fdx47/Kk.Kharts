<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Support</h1>

    <!-- Chat-like interface -->
    <div class="space-y-4 mb-6">
      <!-- Welcome message -->
      <div class="flex gap-3">
        <div class="w-10 h-10 rounded-full bg-kk-primary flex items-center justify-center flex-shrink-0">
          <svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="tg-card flex-1">
          <p class="text-sm">
            Bonjour {{ userName }}! 👋<br><br>
            Comment puis-je vous aider aujourd'hui? Décrivez votre problème ou question et notre équipe vous répondra dès que possible.
          </p>
        </div>
      </div>

      <!-- Previous messages -->
      <div v-for="msg in messages" :key="msg.id" class="flex gap-3" :class="{ 'flex-row-reverse': msg.isUser }">
        <div 
          class="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0"
          :class="msg.isUser ? 'bg-tg-button' : 'bg-kk-primary'">
          <svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="tg-card flex-1" :class="{ 'bg-tg-button text-tg-button-text': msg.isUser }">
          <p class="text-sm">{{ msg.text }}</p>
          <p class="text-xs mt-1 opacity-60">{{ formatTime(msg.timestamp) }}</p>
        </div>
      </div>
    </div>

    <!-- Input area -->
    <div class="fixed bottom-20 inset-x-0 p-4 bg-tg-bg border-t border-tg-secondary-bg">
      <div class="flex gap-2">
        <textarea
          v-model="newMessage"
          ref="inputRef"
          class="flex-1 bg-tg-secondary-bg rounded-xl px-4 py-3 text-sm resize-none"
          :class="{ 'text-tg-text': true }"
          rows="1"
          placeholder="Écrivez votre message..."
          @keydown.enter.prevent="sendMessage"
          @input="autoResize">
        </textarea>
        <button 
          class="w-12 h-12 rounded-full bg-tg-button flex items-center justify-center"
          :class="{ 'opacity-50': !newMessage.trim() || sending }"
          :disabled="!newMessage.trim() || sending"
          @click="sendMessage">
          <svg v-if="!sending" class="w-5 h-5 text-tg-button-text" fill="currentColor" viewBox="0 0 20 20">
            <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z" />
          </svg>
          <div v-else class="w-5 h-5 border-2 border-tg-button-text border-t-transparent rounded-full animate-spin"></div>
        </button>
      </div>
    </div>

    <!-- Quick actions -->
    <div class="mb-24">
      <h3 class="text-sm font-semibold text-tg-hint mb-3">Questions fréquentes</h3>
      <div class="flex flex-wrap gap-2">
        <button 
          v-for="faq in faqs" 
          :key="faq"
          class="bg-tg-secondary-bg px-3 py-2 rounded-full text-sm"
          @click="askFaq(faq)">
          {{ faq }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue'
import { useAppStore } from '../stores/app'

const store = useAppStore()

const newMessage = ref('')
const sending = ref(false)
const messages = ref([])
const inputRef = ref(null)

const userName = computed(() => {
  return store.user?.email || 'Utilisateur'
})

const faqs = [
  'Capteur hors ligne',
  'Batterie faible',
  'Données incorrectes',
  'Ajouter un capteur',
  'Autre problème'
]

async function sendMessage() {
  if (!newMessage.value.trim() || sending.value) return

  const text = newMessage.value.trim()
  newMessage.value = ''
  sending.value = true

  // Add user message to chat
  messages.value.push({
    id: Date.now(),
    text,
    isUser: true,
    timestamp: new Date()
  })

  try {
    const success = await store.sendSupportMessage(text)
    
    if (success) {
      // Add confirmation message
      messages.value.push({
        id: Date.now() + 1,
        text: 'Merci pour votre message! Notre équipe a été notifiée et vous répondra bientôt.',
        isUser: false,
        timestamp: new Date()
      })
    } else {
        // Fallback since API might not send email
        messages.value.push({
        id: Date.now() + 1,
        text: 'Pour le support, veuillez contacter support@kropkontrol.com',
        isUser: false,
        timestamp: new Date()
      })
    }
  } catch (e) {
    messages.value.push({
      id: Date.now() + 1,
      text: 'Désolé, une erreur s\'est produite. Veuillez réessayer.',
      isUser: false,
      timestamp: new Date()
    })
  } finally {
    sending.value = false
  }
}

function askFaq(question) {
  store.hapticFeedback('light')
  newMessage.value = question
  nextTick(() => {
    inputRef.value?.focus()
  })
}

function autoResize(e) {
  const textarea = e.target
  textarea.style.height = 'auto'
  textarea.style.height = Math.min(textarea.scrollHeight, 120) + 'px'
}

function formatTime(date) {
  return new Date(date).toLocaleTimeString('fr-FR', {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>
