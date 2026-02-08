import { configure } from 'quasar/wrappers'

export default configure(function () {
  return {
    eslint: {
      warnings: true,
      errors: true
    },

    boot: [
      'i18n',
      'axios',
      'pwa-push'
    ],

    css: [
      'app.scss'
    ],

    extras: [
      'roboto-font',
      'material-icons',
      'mdi-v7'
    ],

    build: {
      target: {
        browser: ['es2019', 'edge88', 'firefox78', 'chrome87', 'safari13.1'],
        node: 'node16'
      },
      vueRouterMode: 'history',
      env: {
        API_URL: process.env.API_URL || 'https://kropkontrol.premiumasp.net/api/v1',
        VAPID_PUBLIC_KEY: process.env.VAPID_PUBLIC_KEY || ''
      }
    },

    devServer: {
      open: true,
      port: 9000
    },

    framework: {
      config: {
        dark: 'auto',
        brand: {
          primary: '#22c55e',
          secondary: '#3b82f6',
          accent: '#8b5cf6',
          dark: '#1C1C1E',
          'dark-page': '#121212',
          positive: '#22c55e',
          negative: '#ef4444',
          info: '#3b82f6',
          warning: '#f59e0b'
        },
        notify: {
          position: 'top',
          timeout: 3000,
          textColor: 'white'
        },
        loading: {
          spinnerColor: 'primary'
        }
      },
      plugins: [
        'Notify',
        'Dialog',
        'Loading',
        'LocalStorage',
        'SessionStorage',
        'BottomSheet',
        'AppFullscreen'
      ]
    },

    animations: 'all',

    pwa: {
      workboxMode: 'GenerateSW',
      injectPwaMetaTags: true,
      swFilename: 'sw.js',
      manifestFilename: 'manifest.json',
      useCredentialsForManifestTag: false,

      workboxOptions: {
        skipWaiting: true,
        clientsClaim: true,
        runtimeCaching: [
          {
            urlPattern: /^https:\/\/kropkontrol\.premiumasp\.net\/api\/.*/i,
            handler: 'StaleWhileRevalidate',
            options: {
              cacheName: 'api-cache',
              expiration: {
                maxEntries: 100,
                maxAgeSeconds: 60 * 60 // 1 hour
              },
              cacheableResponse: {
                statuses: [0, 200]
              }
            }
          },
          {
            urlPattern: /\.(?:png|jpg|jpeg|svg|gif|webp|ico)$/i,
            handler: 'CacheFirst',
            options: {
              cacheName: 'images-cache',
              expiration: {
                maxEntries: 60,
                maxAgeSeconds: 60 * 60 * 24 * 30 // 30 days
              }
            }
          },
          {
            urlPattern: /\.(?:js|css|woff2?)$/i,
            handler: 'CacheFirst',
            options: {
              cacheName: 'static-cache',
              expiration: {
                maxEntries: 100,
                maxAgeSeconds: 60 * 60 * 24 * 7 // 7 days
              }
            }
          }
        ]
      },

      manifest: {
        name: 'KropKontrol Mobile',
        short_name: 'KropKontrol',
        description: 'IoT Monitoring & Alerting for Agriculture',
        display: 'standalone',
        orientation: 'portrait',
        background_color: '#121212',
        theme_color: '#1C1C1E',
        start_url: '/',
        scope: '/',
        categories: ['utilities', 'productivity'],
        icons: [
          {
            src: 'icons/icon-128x128.png',
            sizes: '128x128',
            type: 'image/png'
          },
          {
            src: 'icons/icon-192x192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: 'icons/icon-256x256.png',
            sizes: '256x256',
            type: 'image/png'
          },
          {
            src: 'icons/icon-384x384.png',
            sizes: '384x384',
            type: 'image/png'
          },
          {
            src: 'icons/icon-512x512.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'any maskable'
          }
        ]
      }
    }
  }
})
