<template>
  <div class="dashboard">
    <NavigationHeader>
      <template #controls>
        <div class="dashboard-controls">
          <div class="date-picker-wrapper">
            <div class="input-group">
              <span class="input-group-text"><i class="bi bi-calendar3"></i></span>
              <input ref="dateInput" type="text" class="form-control form-control-sm date-input"
                :value="selectedDate ? formatDateForDisplay(selectedDate) : ''"
                placeholder="Choisir une date..." readonly @click="openDatePicker" />
            </div>
          </div>
          <div class="user-filter-wrapper" v-if="analytics && analytics.users && analytics.users.length > 0">
            <select class="form-select form-select-sm" v-model="selectedUser" @change="onUserFilterChange">
              <option value="">Tous les utilisateurs</option>
              <option v-for="u in analytics.users" :key="u.name" :value="u.name">
                {{ u.name }} ({{ u.totalRequests }} req)
              </option>
            </select>
          </div>
          <button class="btn btn-primary btn-sm dashboard-refresh" @click="refreshData" :disabled="loading">
            <i class="bi bi-arrow-clockwise me-1" :class="{ 'spin': loading }"></i>Actualiser
          </button>
        </div>
      </template>
    </NavigationHeader>

    <main class="dashboard-main">
      <div class="container-fluid">
        <div v-if="loading && !hasData" class="text-center py-5">
          <div class="spinner-border text-primary" role="status"><span class="visually-hidden">Chargement...</span></div>
          <p class="mt-3 text-muted">Chargement des données...</p>
        </div>

        <div v-else>
          <!-- KPI Row 1: Infrastructure -->
          <div class="row mb-4">
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-devices clickable" @click="openActiveModal">
                <div class="stat-icon"><i class="bi bi-cpu"></i></div>
                <div class="stat-content">
                  <h3>{{ stats.totalDevices }}</h3>
                  <span>Capteurs Actifs</span>
                  <div class="stat-detail">
                    <span class="text-success">{{ stats.onlineDevices }} en ligne</span>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-offline" @click="openOfflineModal" style="cursor:pointer">
                <div class="stat-icon"><i class="bi bi-wifi-off"></i></div>
                <div class="stat-content">
                  <h3>{{ stats.offlineDevices }}</h3>
                  <span>Hors Ligne</span>
                  <div class="stat-detail"><span class="text-warning">{{ stats.inactiveDevices }} inactifs</span></div>
                </div>
              </div>
            </div>
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-users clickable" @click="showUsersModal = true">
                <div class="stat-icon"><i class="bi bi-people"></i></div>
                <div class="stat-content">
                  <h3>{{ stats.totalUsers }}</h3>
                  <span>Utilisateurs</span>
                  <div class="stat-detail">
                    <span class="text-info">{{ stats.linkedTelegramUsers }} Telegram</span>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-alerts">
                <div class="stat-icon"><i class="bi bi-bell"></i></div>
                <div class="stat-content">
                  <h3>{{ stats.activeAlerts }}</h3>
                  <span>Alertes Actives</span>
                  <div class="stat-detail"><span class="text-secondary">{{ stats.totalRules }} règles</span></div>
                </div>
              </div>
            </div>
          </div>

          <!-- KPI Row 2: Log Analytics -->
          <div class="row mb-4" v-if="analytics">
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-requests">
                <div class="stat-icon"><i class="bi bi-arrow-left-right"></i></div>
                <div class="stat-content">
                  <h3>{{ filteredTotalRequests }}</h3>
                  <span>Requêtes API</span>
                  <div class="stat-detail"><span>{{ selectedUser ? 'Filtrées' : 'Total du jour' }}</span></div>
                </div>
              </div>
            </div>
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-unique-users">
                <div class="stat-icon"><i class="bi bi-person-badge"></i></div>
                <div class="stat-content">
                  <h3>{{ analytics.uniqueUsers }}</h3>
                  <span>Utilisateurs Uniques</span>
                  <div class="stat-detail"><span>dans les logs</span></div>
                </div>
              </div>
            </div>
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-endpoints">
                <div class="stat-icon"><i class="bi bi-signpost-split"></i></div>
                <div class="stat-content">
                  <h3>{{ analytics.uniqueEndpoints }}</h3>
                  <span>Endpoints Uniques</span>
                  <div class="stat-detail"><span>routes sollicitées</span></div>
                </div>
              </div>
            </div>
            <div class="col-xl-3 col-lg-6 mb-3">
              <div class="stat-card stat-peak">
                <div class="stat-icon"><i class="bi bi-lightning-charge"></i></div>
                <div class="stat-content">
                  <h3>{{ analytics.peakHour }}</h3>
                  <span>Heure de Pointe</span>
                  <div class="stat-detail"><span>{{ analytics.peakHourRequests }} requêtes</span></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Charts Row -->
          <div class="row mb-4">
            <div class="col-lg-8 mb-3">
              <div class="card chart-card">
                <div class="card-header"><h5 class="mb-0"><i class="bi bi-graph-up me-2"></i>Activité Horaire{{ selectedUser ? ` — ${selectedUser}` : '' }}</h5></div>
                <div class="card-body">
                  <div class="chart-shell chart-shell-lg"><canvas ref="hourlyChart"></canvas></div>
                </div>
              </div>
            </div>
            <div class="col-lg-4 mb-3">
              <div class="card chart-card">
                <div class="card-header"><h5 class="mb-0"><i class="bi bi-pie-chart me-2"></i>Répartition des Méthodes</h5></div>
                <div class="card-body">
                  <div class="chart-shell chart-shell-md"><canvas ref="methodsChart"></canvas></div>
                  <div class="method-breakdown mt-3" v-if="methodBreakdown.length">
                    <div class="method-pill" v-for="entry in methodBreakdown" :key="entry.method">
                      <span class="method-label">{{ entry.method }}</span>
                      <strong>{{ entry.count }}</strong>
                      <small>{{ entry.percentage }}%</small>
                    </div>
                  </div>
                  <p v-else class="text-muted small text-center mb-0">Aucune donnée disponible</p>
                </div>
              </div>
            </div>
          </div>

          <!-- User Activity Bar Chart + User Detail Panel -->
          <div class="row mb-4 align-items-stretch" v-if="analytics && !selectedUser">
            <div class="col-xl-5 col-lg-6 mb-3">
              <div class="card chart-card compact-chart">
                <div class="card-header"><h5 class="mb-0"><i class="bi bi-bar-chart me-2"></i>Requêtes par Utilisateur</h5></div>
                <div class="card-body">
                  <div class="chart-shell chart-shell-md" :style="{ height: usersChartHeight + 'px' }"><canvas ref="usersBarChart"></canvas></div>
                </div>
              </div>
            </div>
            <div class="col-xl-7 col-lg-6 mb-3">
              <div class="card data-card user-mini-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0"><i class="bi bi-person-lines-fill me-2"></i>Détail Utilisateurs</h5>
                  <small class="text-muted">Top utilisateurs · cliquez pour filtrer</small>
                </div>
                <div class="card-body">
                  <div class="user-mini-grid">
                    <div v-for="user in analytics.users.slice(0, 20)" :key="user.name" class="mini-card" @click="selectedUser = user.name; onUserFilterChange()">
                      <div class="d-flex align-items-center gap-2">
                        <div class="mini-avatar" :style="{ background: getAvatarColor(user.name) }">{{ getInitials(user.name) }}</div>
                        <div class="flex-grow-1">
                          <div class="mini-name">{{ user.name }}</div>
                          <div class="mini-meta">{{ user.totalRequests }} req · {{ formatTime(user.firstSeenUtc) }} - {{ formatTime(user.lastSeenUtc) }}</div>
                        </div>
                      </div>
                      <div class="mini-methods">
                        <span v-for="(count, method) in user.methods" :key="method" class="method-chip" :class="getMethodClass(method)">{{ method }} {{ count }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="text-end mt-3" v-if="analytics.users.length > 20">
                    <button class="btn btn-outline-primary btn-sm" @click="showUsersModal = true">
                      Voir tous les utilisateurs ({{ analytics.users.length }})
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Selected User Detail Panel -->
          <div class="row mb-4" v-if="selectedUser && selectedUserData">
            <div class="col-lg-8 mb-3">
              <div class="card chart-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0"><i class="bi bi-person-fill me-2"></i>{{ selectedUser }} — Activité Horaire</h5>
                  <button class="btn btn-outline-secondary btn-sm" @click="selectedUser = ''; onUserFilterChange()"><i class="bi bi-x-lg"></i></button>
                </div>
                <div class="card-body">
                  <div class="chart-shell chart-shell-sm"><canvas ref="userHourlyChart"></canvas></div>
                </div>
              </div>
            </div>
            <div class="col-lg-4 mb-3">
              <div class="card data-card">
                <div class="card-header"><h5 class="mb-0"><i class="bi bi-list-check me-2"></i>Endpoints Utilisés</h5></div>
                <div class="card-body">
                  <div v-for="(count, ep) in selectedUserData.endpoints" :key="ep" class="endpoint-item">
                    <div class="endpoint-path text-truncate" :title="ep">{{ ep }}</div>
                    <span class="badge bg-primary">{{ count }}</span>
                  </div>
                  <div class="mt-3 border-top pt-3">
                    <div class="d-flex justify-content-between mb-1"><span class="text-muted">Première activité</span><strong>{{ formatDateTime(selectedUserData.firstSeenUtc) }}</strong></div>
                    <div class="d-flex justify-content-between mb-1"><span class="text-muted">Dernière activité</span><strong>{{ formatDateTime(selectedUserData.lastSeenUtc) }}</strong></div>
                    <div class="d-flex justify-content-between"><span class="text-muted">Durée session</span><strong>{{ formatSessionTime(sessionDuration(selectedUserData)) }}</strong></div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Top Endpoints Table -->
          <div class="row mb-4" v-if="analytics && analytics.endpoints && analytics.endpoints.length > 0">
            <div class="col-12">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0"><i class="bi bi-signpost-split me-2"></i>Top Endpoints</h5>
                  <span class="badge bg-secondary">{{ analytics.endpoints.length }} routes</span>
                </div>
                <div class="card-body p-0">
                  <div class="table-responsive">
                    <table class="table table-hover mb-0">
                      <thead class="table-light">
                        <tr><th>Endpoint</th><th class="text-center">Requêtes</th><th class="text-center">Méthodes</th><th class="text-center">Utilisateurs</th></tr>
                      </thead>
                      <tbody>
                        <tr v-for="ep in analytics.endpoints.slice(0, 15)" :key="ep.endpoint" class="endpoint-row" @click="openEndpointDetail(ep.endpoint)">
                          <td class="text-truncate" style="max-width:400px" :title="ep.endpoint"><code>{{ ep.endpoint }}</code></td>
                          <td class="text-center"><span class="badge bg-primary">{{ ep.count }}</span></td>
                          <td class="text-center">
                            <span v-for="(c, m) in ep.methods" :key="m" class="method-badge me-1" :class="getMethodClass(m)">{{ m }}: {{ c }}</span>
                          </td>
                          <td class="text-center"><span class="badge bg-secondary">{{ ep.users.length }}</span></td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Devices & Companies Row -->
          <div class="row mb-4">
            <div class="col-lg-4 mb-3">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0"><i class="bi bi-wifi-off text-danger me-2"></i>Capteurs Hors Ligne</h5>
                  <span class="badge bg-danger">{{ offlineDevicesList.length }}</span>
                </div>
                <div class="card-body">
                  <div v-if="offlineDevicesList.length === 0" class="text-center text-muted py-4">
                    <i class="bi bi-check-circle" style="font-size: 2rem;"></i>
                    <p class="mt-2 mb-0">Tous les capteurs sont en ligne</p>
                  </div>
                  <div v-else class="device-list">
                    <div v-for="device in offlineDevicesList.slice(0, 5)" :key="device.devEui" class="device-item offline">
                      <div class="device-icon"><i class="bi bi-cpu"></i></div>
                      <div class="device-info">
                        <h6>{{ device.name }}</h6>
                        <small class="text-muted">{{ device.company }}</small>
                        <div class="device-meta">
                          <span class="badge bg-secondary">{{ device.location }}</span>
                          <span class="text-danger"><i class="bi bi-clock me-1"></i>{{ formatLastSeen(device.lastSendAt) }}</span>
                        </div>
                      </div>
                    </div>
                    <div v-if="offlineDevicesList.length > 5" class="text-center mt-3">
                      <button class="btn btn-outline-danger btn-sm" @click="showOfflineModal = true">Voir tous ({{ offlineDevicesList.length }})</button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-4 mb-3">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0"><i class="bi bi-battery-half text-warning me-2"></i>Batterie Faible (&le;20%)</h5>
                  <span class="badge bg-warning text-dark">{{ lowBatteryDevices.length }}</span>
                </div>
                <div class="card-body">
                  <div v-if="lowBatteryDevices.length === 0" class="text-center text-muted py-4">
                    <i class="bi bi-battery-full" style="font-size: 2rem;"></i>
                    <p class="mt-2 mb-0">Batterie suffisante partout</p>
                  </div>
                  <div v-else>
                    <div v-for="device in lowBatteryDevices.slice(0, 5)" :key="device.devEui" class="battery-item">
                      <div class="battery-icon" :class="getBatteryClass(device.battery)"><i :class="getBatteryIcon(device.battery)"></i></div>
                      <div class="battery-info"><h6>{{ device.name }}</h6><small class="text-muted">{{ device.company }}</small></div>
                      <div class="battery-level"><span class="badge" :class="getBatteryBadgeClass(device.battery)">{{ device.battery }}%</span></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-4 mb-3">
              <div class="card data-card">
                <div class="card-header"><h5 class="mb-0"><i class="bi bi-building me-2"></i>Top Sociétés</h5></div>
                <div class="card-body">
                  <div v-for="(company, index) in topCompanies" :key="company.name" class="company-item">
                    <div class="company-rank">{{ index + 1 }}</div>
                    <div class="company-info">
                      <h6>{{ company.name }}</h6>
                      <div class="progress" style="height: 8px;">
                        <div class="progress-bar" :style="{ width: getCompanyPercentage(company.count) + '%', background: getCompanyColor(index) }"></div>
                      </div>
                    </div>
                    <div class="company-count"><span class="badge bg-primary">{{ company.count }}</span></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Active Devices Modal -->
    <div v-if="showActiveModal" class="modal-overlay" @click.self="showActiveModal = false">
      <div class="modal-panel modal-panel--xl">
        <div class="modal-header">
          <h5 class="mb-0">Capteurs Actifs ({{ activeDevicesList.length }})</h5>
          <button type="button" class="btn-close" @click="showActiveModal = false"></button>
        </div>
        <div class="modal-body">
          <div class="modal-filters">
            <div class="input-group input-group-sm flex-grow-1 me-2">
              <span class="input-group-text"><i class="bi bi-search"></i></span>
              <input type="text" class="form-control" placeholder="Rechercher par nom, DevEUI, entreprise..." v-model="activeSearch" />
            </div>
            <select class="form-select form-select-sm w-auto" v-model="activeStatusFilter">
              <option value="all">Tous les statuts</option>
              <option value="online">En ligne</option>
              <option value="inactive">Inactifs</option>
            </select>
          </div>
          <div class="table-responsive">
            <table class="table table-striped align-middle">
              <thead>
                <tr>
                  <th>Nom</th>
                  <th>DevEUI</th>
                  <th>Entreprise</th>
                  <th>Description</th>
                  <th>Localisation</th>
                  <th>Modèle</th>
                  <th>Statut</th>
                  <th>Dernière comm.</th>
                  <th>Batterie</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="device in filteredActiveDevices" :key="device.devEui">
                  <td>{{ device.name }}</td>
                  <td><code>{{ device.devEui }}</code></td>
                  <td>{{ device.company }}</td>
                  <td>{{ device.description }}</td>
                  <td>{{ device.location }}</td>
                  <td>{{ device.model }}</td>
                  <td>
                    <span class="badge" :class="device.status === 'En ligne' ? 'bg-success' : 'bg-warning text-dark'">{{ device.status }}</span>
                  </td>
                  <td>{{ formatLastSeen(device.lastSendAt) }}</td>
                  <td>{{ device.battery != null ? `${device.battery}%` : '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="modal-footer"><button class="btn btn-secondary" @click="showActiveModal = false">Fermer</button></div>
      </div>
    </div>

    <!-- Offline Modal -->
    <div v-if="showOfflineModal" class="modal-overlay" @click.self="showOfflineModal = false">
      <div class="modal-panel">
        <div class="modal-header">
          <h5 class="mb-0">Capteurs Hors Ligne</h5>
          <button type="button" class="btn-close" @click="showOfflineModal = false"></button>
        </div>
        <div class="modal-body">
          <div class="modal-filters">
            <div class="input-group input-group-sm">
              <span class="input-group-text"><i class="bi bi-search"></i></span>
              <input type="text" class="form-control" placeholder="Rechercher par nom, DevEUI, entreprise..." v-model="offlineSearch" />
            </div>
          </div>
          <table class="table table-striped">
            <thead>
              <tr>
                <th>Nom</th>
                <th>DevEUI</th>
                <th>Entreprise</th>
                <th>Description</th>
                <th>Localisation</th>
                <th>Modèle</th>
                <th>Statut</th>
                <th>Dernière comm.</th>
                <th>Batterie</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="device in filteredOfflineDevices" :key="device.devEui">
                <td>{{ device.name }}</td>
                <td><code>{{ device.devEui }}</code></td>
                <td>{{ device.company }}</td>
                <td>{{ device.description }}</td>
                <td>{{ device.location }}</td>
                <td>{{ device.model }}</td>
                <td><span class="badge" :class="device.status === 'Inactif' ? 'bg-warning text-dark' : 'bg-danger'">{{ device.status }}</span></td>
                <td>{{ formatLastSeen(device.lastSendAt) }}</td>
                <td>{{ device.battery }}%</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="modal-footer"><button class="btn btn-secondary" @click="showOfflineModal = false">Fermer</button></div>
      </div>
    </div>

    <!-- Endpoint Detail Modal -->
    <div v-if="showEndpointModal && endpointDetail" class="modal-overlay" @click.self="closeEndpointModal">
      <div class="modal-panel modal-panel--xl">
        <div class="modal-header d-flex justify-content-between align-items-center">
          <div>
            <h5 class="mb-1">Endpoint</h5>
            <code class="text-wrap">{{ endpointDetail.endpoint }}</code>
          </div>
          <button type="button" class="btn-close" @click="closeEndpointModal"></button>
        </div>
        <div class="modal-body">
          <div class="endpoint-meta" v-if="endpointStats.total > 0 || endpointStats.userCount > 0">
            <div class="meta-card">
              <span>Total requêtes</span>
              <strong>{{ endpointStats.total }}</strong>
            </div>
            <div class="meta-card">
              <span>Utilisateurs actifs</span>
              <strong>{{ endpointStats.userCount }}</strong>
            </div>
            <div class="meta-card">
              <span>Heure de pointe</span>
              <strong>{{ endpointStats.peakHour }} <small v-if="endpointStats.peakValue">({{ endpointStats.peakValue }} req)</small></strong>
            </div>
          </div>
          <div class="row">
            <div class="col-lg-8 mb-3">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h6 class="mb-0"><i class="bi bi-table me-2"></i>Résumé horaire</h6>
                  <small class="text-muted">Chronologie de la journée</small>
                </div>
                <div class="card-body p-0">
                  <div v-if="endpointHourlyRows.length" class="table-responsive">
                    <table class="table table-hover table-sm mb-0">
                      <thead>
                        <tr><th>Heure</th><th class="text-center">Requêtes</th><th>Utilisateur dominant</th></tr>
                      </thead>
                      <tbody>
                        <tr v-for="row in endpointHourlyRows" :key="row.hour">
                          <td>{{ row.hourLabel }}</td>
                          <td class="text-center"><span class="badge bg-primary-subtle text-primary fw-semibold">{{ row.count }}</span></td>
                          <td>{{ row.topUser || '—' }}</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                  <div v-else class="empty-block">Aucune activité répertoriée pour cet endpoint aujourd'hui.</div>
                </div>
              </div>
              <div class="card chart-card mt-3">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Activité horaire (Total vs utilisateurs)</h6>
                </div>
                <div class="card-body chart-holder">
                  <div v-if="!hasEndpointChartData" class="chart-placeholder">
                    <i class="bi bi-activity"></i>
                    <p class="mb-0">Aucune activité horaire disponible pour cette date.</p>
                  </div>
                  <canvas v-else ref="endpointHourlyChart" height="240"></canvas>
                </div>
              </div>
              <div class="card data-card method-card mt-3" v-if="endpointMethodEntries.length">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h6 class="mb-0"><i class="bi bi-signpost me-2"></i>Méthodes HTTP</h6>
                  <small class="text-muted">Requêtes totales</small>
                </div>
                <div class="card-body">
                  <div class="method-chip" v-for="method in endpointMethodEntries" :key="method.method">
                    {{ method.method }} · {{ method.count }}
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-4 mb-3">
              <div class="card data-card">
                <div class="card-header"><h6 class="mb-0"><i class="bi bi-people me-2"></i>Détail Utilisateurs</h6></div>
                <div class="card-body p-0">
                  <div v-for="user in endpointDetail.users" :key="user.user" class="endpoint-user-panel">
                    <div class="user-panel__header">
                      <div>
                        <h6 class="mb-1">{{ user.user }}</h6>
                        <div class="stat-line">
                          <span class="stat-chip">{{ sumArray(user.hourlyCounts) }} req</span>
                          <span class="stat-chip">{{ getUserShare(user) }}% du total</span>
                        </div>
                        <div class="small text-muted" v-if="user.firstSeenUtc && user.lastSeenUtc">
                          {{ formatTime(user.firstSeenUtc) }} → {{ formatTime(user.lastSeenUtc) }}
                        </div>
                      </div>
                      <span class="badge rounded-pill bg-primary-subtle text-primary fw-semibold">Pic {{ user.peakHourLabel }}</span>
                    </div>
                    <div v-if="userHourEntries(user).length" class="user-hour-table">
                      <table class="table table-sm mb-0">
                        <thead>
                          <tr><th>Heure</th><th>Volume</th><th class="text-end">Poids</th></tr>
                        </thead>
                        <tbody>
                          <tr v-for="entry in userHourEntries(user)" :key="entry.hour">
                            <td>{{ entry.hourLabel }}</td>
                            <td class="w-100">
                              <div class="user-hour-row__bar">
                                <div class="user-hour-row__bar-fill" :style="{ width: entry.percent + '%' }"></div>
                              </div>
                            </td>
                            <td class="text-end fw-semibold">{{ entry.count }}<small class="text-muted"> ({{ entry.share }}%)</small></td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                    <div class="text-muted small px-3 pb-3" v-else>Aucune activité horodatée.</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer"><button class="btn btn-secondary" @click="closeEndpointModal">Fermer</button></div>
      </div>
    </div>

    <!-- Users Modal -->
    <div v-if="showUsersModal" class="modal-overlay" @click.self="showUsersModal = false">
      <div class="modal-panel modal-panel--xl">
        <div class="modal-header">
          <h5 class="mb-0">Utilisateurs ({{ usersList.length }})</h5>
          <button type="button" class="btn-close" @click="showUsersModal = false"></button>
        </div>
        <div class="modal-body">
          <div class="table-responsive">
            <table class="table table-striped align-middle">
              <thead>
                <tr>
                  <th>Nom</th>
                  <th>Email</th>
                  <th>Rôle</th>
                  <th>Société</th>
                  <th>Téléphone</th>
                  <th>Dernière connexion</th>
                  <th>Telegram</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in usersList" :key="user.email + user.name">
                  <td>{{ user.name }}</td>
                  <td><a :href="`mailto:${user.email}`">{{ user.email }}</a></td>
                  <td><span class="badge bg-primary">{{ user.role }}</span></td>
                  <td>{{ user.company }}</td>
                  <td>{{ user.phone || '—' }}</td>
                  <td>{{ formatDateTime(user.lastLogin) }}</td>
                  <td>
                    <span class="badge" :class="user.telegram ? 'bg-success' : 'bg-secondary'">
                      {{ user.telegram ? 'Oui' : 'Non' }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div class="modal-footer"><button class="btn btn-secondary" @click="showUsersModal = false">Fermer</button></div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { deviceService, logsService, userService } from '@/services/api'
import { toast } from 'vue3-toastify'
import flatpickr from 'flatpickr'
import 'flatpickr/dist/flatpickr.min.css'
import { French } from 'flatpickr/dist/l10n/fr'
import Chart from 'chart.js/auto'
import NavigationHeader from '@/components/NavigationHeader.vue'

export default {
  name: 'DashboardView',
  components: { NavigationHeader },
  setup() {
    const router = useRouter()
    const loading = ref(false)
    const selectedDate = ref('')
    const selectedUser = ref('')
    const dateInput = ref(null)
    const hourlyChart = ref(null)
    const methodsChart = ref(null)
    const usersBarChart = ref(null)
    const userHourlyChart = ref(null)
    const endpointHourlyChart = ref(null)
    const showOfflineModal = ref(false)
    const showActiveModal = ref(false)
    const showUsersModal = ref(false)
    const showEndpointModal = ref(false)
    const endpointDetail = ref(null)
    const activeSearch = ref('')
    const activeStatusFilter = ref('all')
    const offlineSearch = ref('')
    let flatpickrInstance = null
    let hourlyChartInstance = null
    let methodsChartInstance = null
    let usersBarChartInstance = null
    let userHourlyChartInstance = null
    let endpointHourlyChartInstance = null

    const stats = ref({ totalDevices: 0, onlineDevices: 0, offlineDevices: 0, inactiveDevices: 0, lowBatteryDevices: 0, activeAlerts: 0, totalRules: 0, totalUsers: 0, linkedTelegramUsers: 0 })
    const analytics = ref(null)
    const deviceModels = ref([])
    const activeDevicesList = ref([])
    const offlineDevicesList = ref([])
    const lowBatteryDevices = ref([])
    const topCompanies = ref([])
    const usersList = ref([])
    const endpointHourlyMap = ref(new Map())
    const filteredActiveDevices = ref([])
    const filteredOfflineDevices = ref([])
    const methodBreakdown = ref([])
    const prepareCanvasContext = (canvas, height = 240) => {
      if (!canvas) return null
      const parentWidth = canvas.parentElement?.clientWidth || canvas.getBoundingClientRect().width || 600
      canvas.style.width = parentWidth + 'px'
      canvas.style.height = `${height}px`
      canvas.width = parentWidth
      canvas.height = height
      return canvas.getContext('2d')
    }

    const hasData = computed(() => stats.value.totalDevices > 0 || analytics.value !== null)
    const modelLookup = computed(() => {
      const map = new Map()
      deviceModels.value.forEach(model => {
        if (!model) return
        if (model.modelId != null) map.set(Number(model.modelId), model)
        if (model.model) map.set(String(model.model).toLowerCase(), model)
      })
      return map
    })

    const filteredTotalRequests = computed(() => {
      if (!analytics.value) return 0
      if (!selectedUser.value) return analytics.value.totalRequests
      const u = analytics.value.users.find(x => x.name === selectedUser.value)
      return u ? u.totalRequests : 0
    })

    const selectedUserData = computed(() => {
      if (!analytics.value || !selectedUser.value) return null
      return analytics.value.users.find(x => x.name === selectedUser.value) || null
    })

    const sessionDuration = (userData) => {
      if (!userData?.firstSeenUtc || !userData?.lastSeenUtc) return 0
      return new Date(userData.lastSeenUtc) - new Date(userData.firstSeenUtc)
    }

    const usersChartHeight = computed(() => {
      const count = analytics.value?.users?.length || 0
      if (count === 0) return 220
      if (count <= 10) return 240
      if (count <= 20) return 320
      if (count <= 35) return 420
      return 520
    })

    const applyActiveFilters = () => {
      const search = activeSearch.value.trim().toLowerCase()
      const statusFilter = activeStatusFilter.value
      filteredActiveDevices.value = activeDevicesList.value.filter(device => {
        const matchesText = !search || `${device.name} ${device.devEui} ${device.company} ${device.location}`.toLowerCase().includes(search)
        const matchesStatus = statusFilter === 'all'
          || (statusFilter === 'online' && device.status === 'En ligne')
          || (statusFilter === 'inactive' && device.status !== 'En ligne')
        return matchesText && matchesStatus
      })
    }

    const applyOfflineFilters = () => {
      const search = offlineSearch.value.trim().toLowerCase()
      filteredOfflineDevices.value = offlineDevicesList.value.filter(device => !search || `${device.name} ${device.devEui} ${device.company} ${device.location}`.toLowerCase().includes(search))
    }

    watch([activeDevicesList, activeSearch, activeStatusFilter], applyActiveFilters, { immediate: true })
    watch([offlineDevicesList, offlineSearch], applyOfflineFilters, { immediate: true })

    const openActiveModal = () => {
      activeSearch.value = ''
      activeStatusFilter.value = 'all'
      showActiveModal.value = true
    }

    const openOfflineModal = () => {
      offlineSearch.value = ''
      showOfflineModal.value = true
    }

    const formatDateForDisplay = (dateStr) => {
      if (!dateStr) return ''
      const [year, month, day] = dateStr.split('-')
      return `${day}/${month}/${year}`
    }

    const openDatePicker = () => {
      if (flatpickrInstance) { flatpickrInstance.open(); return }
      flatpickrInstance = flatpickr(dateInput.value, {
        locale: French, dateFormat: 'd/m/Y', maxDate: 'today',
        defaultDate: selectedDate.value ? new Date(selectedDate.value) : new Date(),
        onChange: (selectedDates) => {
          if (selectedDates.length > 0) {
            const formatted = selectedDates[0].toISOString().split('T')[0]
            if (selectedDate.value !== formatted) selectedDate.value = formatted
          }
        }
      })
      flatpickrInstance.open()
    }

    const isDeviceOnline = (device) => {
      const activeFlag = device?.activeInKropKontrol ?? device?.ActiveInKropKontrol
      const lastSendAt = device?.lastSendAt ?? device?.LastSendAt
      const isRecent = lastSendAt && (lastSendAt === "À l'instant" || lastSendAt.includes('seconde') || (lastSendAt.includes('minute') && parseInt(lastSendAt.match(/\d+/)?.[0] || 0) <= 15))
      return activeFlag === true && isRecent
    }

    const onUserFilterChange = () => { nextTick(() => updateCharts()) }

    const refreshData = async (showSuccessToast = true) => {
      loading.value = true
      try {
        const date = selectedDate.value || new Date().toISOString().split('T')[0]
        const [devicesRaw, modelsRaw, usersRaw, analyticsData] = await Promise.all([
          deviceService.getAll(),
          deviceService.getModels().catch(() => []),
          userService.getAll(),
          logsService.fetchAnalytics(date).catch(() => null)
        ])

        const devices = Array.isArray(devicesRaw) ? devicesRaw : []
        const models = Array.isArray(modelsRaw) ? modelsRaw : []
        const users = Array.isArray(usersRaw) ? usersRaw : []
        deviceModels.value = models

        const get = (d, k) => d?.[k] ?? d?.[k.charAt(0).toUpperCase() + k.slice(1)]
        const resolveModelLabel = (device) => {
          const raw = get(device, 'deviceModel') ?? get(device, 'model') ?? get(device, 'modelId')
          if (raw && typeof raw === 'object') {
            return raw.name || raw.model || raw.description || '—'
          }
          if (typeof raw === 'number') {
            const info = modelLookup.value.get(raw)
            return info?.model || info?.description || `Modèle #${raw}`
          }
          if (typeof raw === 'string' && raw.trim().length > 0) {
            const info = modelLookup.value.get(raw.trim().toLowerCase())
            return info?.model || raw
          }
          const numericFallback = Number(raw)
          if (!Number.isNaN(numericFallback)) {
            const info = modelLookup.value.get(numericFallback)
            if (info) return info.model || info.description || `Modèle #${numericFallback}`
          }
          return '—'
        }

        const inactive = devices.filter(d => get(d, 'activeInKropKontrol') === false)
        const active = devices.filter(d => get(d, 'activeInKropKontrol') !== false)
        const online = active.filter(isDeviceOnline)
        const offline = active.filter(d => !isDeviceOnline(d))

        stats.value.totalDevices = active.length
        stats.value.onlineDevices = online.length
        stats.value.offlineDevices = offline.length
        stats.value.inactiveDevices = inactive.length
        stats.value.lowBatteryDevices = active.filter(d => typeof get(d, 'battery') === 'number' && get(d, 'battery') <= 20).length
        stats.value.totalUsers = users.length
        stats.value.linkedTelegramUsers = 0
        stats.value.activeAlerts = 0
        stats.value.totalRules = 0

        activeDevicesList.value = active.map(d => ({
          name: get(d, 'name') || get(d, 'devEui'),
          company: get(d, 'companyName') || 'N/A',
          description: get(d, 'description') || '—',
          location: get(d, 'installationLocation') || '—',
          devEui: get(d, 'devEui'),
          model: resolveModelLabel(d),
          status: isDeviceOnline(d) ? 'En ligne' : 'Inactif',
          lastSendAt: get(d, 'lastSendAt'),
          battery: get(d, 'battery')
        }))

        offlineDevicesList.value = offline.map(d => ({
          name: get(d, 'name') || get(d, 'devEui'),
          company: get(d, 'companyName') || 'N/A',
          description: get(d, 'description') || '—',
          location: get(d, 'installationLocation') || '—',
          devEui: get(d, 'devEui'),
          lastSendAt: get(d, 'lastSendAt'),
          battery: get(d, 'battery'),
          model: resolveModelLabel(d),
          status: get(d, 'activeInKropKontrol') === false ? 'Inactif' : 'Hors ligne'
        }))

        lowBatteryDevices.value = active
          .filter(d => typeof get(d, 'battery') === 'number' && get(d, 'battery') <= 20)
          .map(d => ({ name: get(d, 'name') || get(d, 'devEui'), company: get(d, 'companyName') || 'N/A', devEui: get(d, 'devEui'), battery: get(d, 'battery') }))

        const companyCounts = new Map()
        active.forEach(d => { const c = get(d, 'companyName') || 'N/A'; companyCounts.set(c, (companyCounts.get(c) || 0) + 1) })
        topCompanies.value = Array.from(companyCounts.entries()).map(([name, count]) => ({ name, count })).sort((a, b) => b.count - a.count).slice(0, 10)

        usersList.value = users.map(u => ({
          name: get(u, 'name') || get(u, 'nom') || get(u, 'username') || 'Utilisateur',
          email: get(u, 'email') || '—',
          role: classifyUserRole(get(u, 'role')),
          company: get(u, 'companyName') || get(u, 'societeNom') || '—',
          phone: get(u, 'phone') || get(u, 'telephone') || null,
          lastLogin: get(u, 'lastLogin') || get(u, 'lastLoginAt') || get(u, 'lastConnexion'),
          telegram: Boolean(get(u, 'telegramUserId') || get(u, 'telegramId'))
        }))

        stats.value.linkedTelegramUsers = usersList.value.filter(u => u.telegram).length

        analytics.value = analyticsData
        endpointHourlyMap.value = buildEndpointMap(analyticsData)
        await nextTick()
        updateCharts()
        if (showSuccessToast) {
          toast.success('Données actualisées')
        }
      } catch (error) {
        toast.error('Erreur: ' + error.message)
      } finally {
        loading.value = false
      }
    }

    const updateCharts = () => {
      if (!analytics.value) return

      const userData = selectedUser.value ? analytics.value.users.find(u => u.name === selectedUser.value) : null
      const hourlyData = userData ? userData.hourlyActivity : analytics.value.hourlyActivity.map(h => h.count)
      const methodData = userData ? userData.methods : Object.fromEntries(analytics.value.methods.map(m => [m.method, m.count]))

      // Hourly Chart
      if (hourlyChartInstance) hourlyChartInstance.destroy()
      if (hourlyChart.value) {
        const labels = Array.from({ length: 24 }, (_, i) => `${i}h`)
        const methodHourly = analytics.value?.methodHourlyActivity || []
        const hourlyCtx = prepareCanvasContext(hourlyChart.value, 220)
        if (!hourlyCtx) return

        if (selectedUser.value && userData) {
          hourlyChartInstance = new Chart(hourlyCtx, {
            type: 'line',
            data: {
              labels,
              datasets: [{
                label: selectedUser.value,
                data: userData.hourlyActivity,
                borderColor: '#667eea',
                backgroundColor: 'rgba(102,126,234,0.15)',
                fill: true,
                tension: 0.4,
                pointRadius: 3
              }]
            },
            options: { animation: false, responsive: false, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
          })
        } else {
          const getCounts = methodHourly.find(m => m.method === 'GET')?.hourlyCounts || Array(24).fill(0)
          const postCounts = methodHourly.find(m => m.method === 'POST')?.hourlyCounts || Array(24).fill(0)
          const totalDataset = {
            label: 'Total',
            data: hourlyData,
            backgroundColor: 'rgba(102, 126, 234, 0.3)',
            borderColor: '#667eea',
            borderWidth: 1,
            borderRadius: 6
          }
          const methodDatasets = [
            {
              label: 'GET',
              data: getCounts,
              backgroundColor: 'rgba(102, 126, 234, 0.9)',
              borderRadius: 6
            },
            {
              label: 'POST',
              data: postCounts,
              backgroundColor: 'rgba(79, 172, 254, 0.9)',
              borderRadius: 6
            }
          ]

          hourlyChartInstance = new Chart(hourlyCtx, {
            type: 'bar',
            data: {
              labels,
              datasets: [totalDataset, ...methodDatasets]
            },
            options: {
              animation: false,
              responsive: false,
              maintainAspectRatio: false,
              plugins: { legend: { position: 'top' } },
              scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
          })
        }
      }

      // Methods Chart
      if (methodsChartInstance) methodsChartInstance.destroy()
      if (methodsChart.value) {
        const orderedMethods = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH']
        const dynamicMethods = Object.keys(methodData).filter(m => !orderedMethods.includes(m))
        const labels = [...orderedMethods, ...dynamicMethods]
        const data = labels.map(label => methodData[label] || 0)
        const palette = ['#667eea', '#4facfe', '#f093fb', '#fa709a', '#30cfd0', '#ffd166', '#06d6a0', '#ff6b6b']
        const total = data.reduce((sum, value) => sum + value, 0)
        methodBreakdown.value = labels
          .map((label, idx) => ({
            method: label,
            count: data[idx],
            percentage: total ? Math.round((data[idx] / total) * 100) : 0,
            color: palette[idx % palette.length]
          }))
          .filter(entry => entry.count > 0)
        const methodsCtx = prepareCanvasContext(methodsChart.value, 220)
        methodsChartInstance = new Chart(methodsCtx, {
          type: 'doughnut',
          data: {
            labels,
            datasets: [{
              data,
              backgroundColor: labels.map((_, idx) => palette[idx % palette.length]),
              borderWidth: 0
            }]
          },
          options: {
            animation: false,
            responsive: false,
            maintainAspectRatio: false,
            cutout: '55%',
            plugins: {
              legend: {
                position: 'bottom',
                labels: { usePointStyle: true, pointStyle: 'circle' }
              },
              tooltip: {
                callbacks: {
                  label: (ctx) => `${ctx.label}: ${ctx.parsed}`
                }
              }
            }
          }
        })
      } else {
        methodBreakdown.value = []
      }

      // Users Bar Chart (only when no user selected)
      if (!selectedUser.value && usersBarChart.value) {
        if (usersBarChartInstance) usersBarChartInstance.destroy()
        const topUsers = analytics.value.users.slice(0, 10)
        const usersCtx = prepareCanvasContext(usersBarChart.value, 240)
        usersBarChartInstance = new Chart(usersCtx, {
          type: 'bar',
          data: {
            labels: topUsers.map(u => u.name.length > 16 ? u.name.substring(0, 16) + '…' : u.name),
            datasets: [
              { label: 'GET', data: topUsers.map(u => u.methods?.GET || 0), backgroundColor: '#667eea' },
              { label: 'POST', data: topUsers.map(u => u.methods?.POST || 0), backgroundColor: '#4facfe' },
              { label: 'PUT', data: topUsers.map(u => u.methods?.PUT || 0), backgroundColor: '#f093fb' },
              { label: 'DELETE', data: topUsers.map(u => u.methods?.DELETE || 0), backgroundColor: '#fa709a' }
            ]
          },
          options: { animation: false, responsive: false, maintainAspectRatio: false, plugins: { legend: { position: 'top' } }, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true, ticks: { precision: 0 } } } }
        })
      }

      // User Hourly Chart (when user selected)
      if (selectedUser.value && userData && userHourlyChart.value) {
        if (userHourlyChartInstance) userHourlyChartInstance.destroy()
        const userHourlyCtx = prepareCanvasContext(userHourlyChart.value, 200)
        userHourlyChartInstance = new Chart(userHourlyCtx, {
          type: 'line',
          data: {
            labels: Array.from({ length: 24 }, (_, i) => `${i}h`),
            datasets: [{ label: selectedUser.value, data: userData.hourlyActivity, borderColor: '#667eea', backgroundColor: 'rgba(102,126,234,0.15)', fill: true, tension: 0.4, pointRadius: 3 }]
          },
          options: { animation: false, responsive: false, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } }
        })
      }

      if (showEndpointModal.value) {
        renderEndpointChart()
      }
    }

    // Helpers
    const formatLastSeen = (v) => { if (!v) return 'Jamais'; const p = Date.parse(v); if (isNaN(p)) return v; const d = Math.floor((Date.now() - p) / 3600000); return d > 24 ? `Il y a ${Math.floor(d/24)}j` : d > 0 ? `Il y a ${d}h` : 'Récemment' }
    const formatTime = (v) => v ? new Date(v).toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' }) : '—'
    const formatDateTime = (v) => v ? new Date(v).toLocaleString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' }) : '—'
    const formatSessionTime = (ms) => { if (!ms || ms < 0) return '0s'; const s = Math.floor(ms/1000); const m = Math.floor(s/60); const h = Math.floor(m/60); return h > 0 ? `${h}h ${m%60}m` : m > 0 ? `${m}m` : `${s}s` }
    const classifyUserRole = (role) => {
      if (!role) return 'Utilisateur'
      const normalized = role.toLowerCase()
      if (normalized.includes('root')) return 'Root'
      if (normalized.includes('admin')) return 'Admin'
      if (normalized.includes('tech')) return 'Technicien'
      if (normalized.includes('demo')) return 'Démo'
      return role
    }
    const getInitials = (n) => n ? n.split(' ').map(p => p[0]).join('').substring(0, 2).toUpperCase() : '?'
    const getAvatarColor = (n) => { const c = ['#667eea','#f093fb','#4facfe','#fa709a','#30cfd0','#a8edea']; return c[n.charCodeAt(0) % c.length] }
    const getMethodClass = (m) => ({ GET: 'method-get', POST: 'method-post', PUT: 'method-put', DELETE: 'method-delete' }[m?.toUpperCase()] || '')
    const getCompanyPercentage = (c) => { const max = topCompanies.value[0]?.count || 1; return (c / max) * 100 }
    const getCompanyColor = (i) => ['#667eea','#f093fb','#4facfe','#fa709a','#30cfd0'][i % 5]
    const getBatteryClass = (l) => l <= 10 ? 'battery-critical' : l <= 20 ? 'battery-low' : 'battery-ok'
    const getBatteryIcon = (l) => l <= 10 ? 'bi bi-battery' : l <= 25 ? 'bi bi-battery-half' : 'bi bi-battery-full'
    const getBatteryBadgeClass = (l) => l <= 10 ? 'bg-danger' : l <= 20 ? 'bg-warning text-dark' : 'bg-success'
    const sumArray = (arr) => arr?.reduce((acc, val) => acc + val, 0) || 0
    const endpointStats = computed(() => {
      if (!endpointDetail.value) return { total: 0, userCount: 0, peakHour: '—', peakValue: 0 }
      const total = sumArray(endpointDetail.value.totalHourlyCounts)
      const userCount = endpointDetail.value.users?.length || 0
      const hourlyCounts = endpointDetail.value.totalHourlyCounts || []
      let peakHour = '—'
      let peakValue = 0
      hourlyCounts.forEach((count, hour) => {
        if (count > peakValue) {
          peakValue = count
          peakHour = `${hour}h`
        }
      })
      return { total, userCount, peakHour, peakValue }
    })

    const hasEndpointChartData = computed(() => {
      if (!endpointDetail.value) return false
      return (endpointDetail.value.totalHourlyCounts || []).some(count => count > 0)
    })

    const endpointHourlyRows = computed(() => {
      if (!endpointDetail.value) return []
      const totals = endpointDetail.value.totalHourlyCounts || []
      return totals
        .map((count, hour) => {
          const topEntry = (endpointDetail.value.users || [])
            .map(user => ({ name: user.user, count: (user.hourlyCounts || [])[hour] || 0 }))
            .sort((a, b) => b.count - a.count)[0]
          return {
            hour,
            hourLabel: `${hour}h`,
            count,
            topUser: topEntry?.count ? `${topEntry.name} (${topEntry.count})` : null
          }
        })
        .filter(row => row.count > 0)
        .sort((a, b) => a.hour - b.hour)
    })

    const endpointMethodEntries = computed(() => {
      if (!endpointDetail.value?.methods) return []
      return Object.entries(endpointDetail.value.methods)
        .map(([method, count]) => ({ method, count }))
        .sort((a, b) => b.count - a.count)
    })

    const userHourEntries = (user) => {
      const counts = user?.hourlyCounts || []
      const entries = counts
        .map((count, hour) => ({ hour, count }))
        .filter(entry => entry.count > 0)
        .sort((a, b) => a.hour - b.hour)
      const max = entries[0]?.count || 1
      const userTotal = sumArray(user?.hourlyCounts)
      return entries.map(entry => ({
        ...entry,
        percent: Math.max((entry.count / max) * 100, 8),
        hourLabel: `${entry.hour}h`,
        share: Math.round((entry.count / (userTotal || 1)) * 100)
      }))
    }

    const getUserShare = (user) => {
      const total = endpointStats.value.total || 1
      const userTotal = sumArray(user?.hourlyCounts)
      return Math.round((userTotal / total) * 100)
    }

    const safeHourlyArray = (arr) => Array.isArray(arr) ? arr : Array(24).fill(0)

    const buildEndpointMap = (analyticsData) => {
      const map = new Map()
      if (!analyticsData?.endpointHourlyBreakdown) return map
      analyticsData.endpointHourlyBreakdown.forEach(item => {
        map.set(item.endpoint, {
          endpoint: item.endpoint,
          totalHourlyCounts: safeHourlyArray(item.totalHourlyCounts),
          users: (item.users || []).map(u => ({
            user: u.user,
            hourlyCounts: safeHourlyArray(u.hourlyCounts),
            peakHourLabel: formatPeakHour(safeHourlyArray(u.hourlyCounts))
          }))
        })
      })
      return map
    }

    const formatPeakHour = (counts) => {
      if (!counts?.length) return '—'
      let max = 0
      let index = 0
      counts.forEach((value, hour) => {
        if (value > max) {
          max = value
          index = hour
        }
      })
      return max > 0 ? `${index}h` : '—'
    }

    const openEndpointDetail = (endpoint) => {
      const detail = endpointHourlyMap.value.get(endpoint)
      if (!detail) return
      const endpointMeta = analytics.value?.endpoints?.find(e => e.endpoint === endpoint)
      endpointDetail.value = {
        ...detail,
        methods: endpointMeta?.methods || {},
        count: endpointMeta?.count || sumArray(detail.totalHourlyCounts)
      }
      showEndpointModal.value = true
      methodBreakdown.value = []
      nextTick(renderEndpointChart)
    }

    const closeEndpointModal = () => {
      showEndpointModal.value = false
      endpointDetail.value = null
      methodBreakdown.value = []
      if (endpointHourlyChartInstance) {
        endpointHourlyChartInstance.destroy()
        endpointHourlyChartInstance = null
      }
    }

    const renderEndpointChart = () => {
      if (!endpointDetail.value || !endpointHourlyChart.value) return
      const labels = Array.from({ length: 24 }, (_, i) => `${i}h`)
      if (endpointHourlyChartInstance) endpointHourlyChartInstance.destroy()

      const canvas = endpointHourlyChart.value
      const parentWidth = canvas.parentElement?.clientWidth || 600
      canvas.width = parentWidth
      canvas.height = 260

      const datasets = [
        {
          label: 'Total',
          data: endpointDetail.value.totalHourlyCounts,
          backgroundColor: 'rgba(102,126,234,0.2)',
          borderColor: '#667eea',
          type: 'bar',
          borderWidth: 1,
          order: 2,
          borderRadius: 6
        }
      ]

      endpointDetail.value.users.slice(0, 4).forEach((user, idx) => {
        const palette = ['#667eea', '#4facfe', '#f093fb', '#fa709a']
        datasets.push({
          label: user.user,
          data: user.hourlyCounts,
          borderColor: palette[idx % palette.length],
          backgroundColor: palette[idx % palette.length] + '33',
          type: 'line',
          tension: 0.35,
          fill: false,
          order: 1
        })
      })

      endpointHourlyChartInstance = new Chart(canvas.getContext('2d'), {
        type: 'bar',
        data: {
          labels,
          datasets
        },
        options: {
          animation: false,
          responsive: false,
          maintainAspectRatio: false,
          scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
          plugins: { legend: { position: 'top' } }
        }
      })
    }

    onMounted(() => {
      selectedDate.value = new Date().toISOString().split('T')[0]
      nextTick(() => {
        if (dateInput.value) {
          flatpickrInstance = flatpickr(dateInput.value, {
            locale: French, dateFormat: 'd/m/Y', maxDate: 'today', defaultDate: new Date(),
            onChange: (selectedDates) => { if (selectedDates.length > 0) { const f = selectedDates[0].toISOString().split('T')[0]; if (selectedDate.value !== f) selectedDate.value = f } }
          })
        }
      })
    })

    const initialLoad = ref(true)
    watch(selectedDate, (v) => {
      if (!v) return
      refreshData(initialLoad.value ? false : true)
      if (initialLoad.value) initialLoad.value = false
    })

    return {
      loading, selectedDate, selectedUser, dateInput, hourlyChart, methodsChart, usersBarChart, userHourlyChart, endpointHourlyChart,
      showOfflineModal, showActiveModal, showUsersModal, showEndpointModal,
      stats, analytics, methodBreakdown, activeDevicesList, offlineDevicesList, lowBatteryDevices, topCompanies, usersList,
      hasData, filteredTotalRequests, selectedUserData, sessionDuration,
      filteredActiveDevices, filteredOfflineDevices, activeSearch, activeStatusFilter, offlineSearch,
      openActiveModal, openOfflineModal,
      endpointDetail, openEndpointDetail, closeEndpointModal,
      endpointStats, hasEndpointChartData, endpointHourlyRows, endpointMethodEntries,
      sumArray, userHourEntries, getUserShare,
      formatDateForDisplay, openDatePicker, refreshData, onUserFilterChange,
      formatLastSeen, formatTime, formatDateTime, formatSessionTime,
      getInitials, getAvatarColor, getMethodClass, getCompanyPercentage, getCompanyColor,
      getBatteryClass, getBatteryIcon, getBatteryBadgeClass
    }
  }
}
</script>

<style scoped>
.dashboard { 
  min-height: 100vh; 
  background: radial-gradient(circle at 20% 10%, #e0e7ff 0%, #f0f4ff 25%, #fafbff 50%, #f8fafc 100%);
  position: relative;
}

.dashboard::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 300px;
  background: linear-gradient(135deg, rgba(99, 102, 241, 0.1) 0%, rgba(139, 92, 246, 0.08) 50%, rgba(236, 72, 153, 0.05) 100%);
  pointer-events: none;
}

.dashboard-main { 
  padding: 2rem 0; 
  position: relative;
  z-index: 1;
}
.dashboard-controls { display: flex; align-items: center; justify-content: flex-end; flex-wrap: wrap; gap: 0.75rem; }
.date-picker-wrapper { display: inline-block; }
.date-input { width: 180px; cursor: pointer; }
.user-filter-wrapper select { min-width: 220px; }
.dashboard-refresh { min-width: 150px; border: none; border-radius: 999px; font-weight: 600; box-shadow: 0 8px 16px rgba(102, 126, 234, 0.35); padding: 0.45rem 1.5rem; display: inline-flex; align-items: center; justify-content: center; gap: 0.35rem; background: linear-gradient(135deg, #1e3c72, #2a5298); }
.dashboard-refresh:hover { background: linear-gradient(135deg, #1b3260, #244987); box-shadow: 0 10px 20px rgba(46, 100, 255, 0.35); }
.dashboard-refresh:disabled { opacity: 0.65; box-shadow: none; }
.chart-shell { width: 100%; border-radius: 14px; background: #f8fafc; padding: 0.5rem; box-shadow: inset 0 0 0 1px rgba(148,163,184,0.2); position: relative; }
.chart-shell canvas { width: 100% !important; height: 100% !important; display: block; }
.chart-shell-lg { height: 260px; }
.chart-shell-md { height: 220px; }
.chart-shell-sm { height: 200px; }
.method-breakdown { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.method-pill { display: inline-flex; align-items: center; gap: 0.4rem; border-radius: 999px; padding: 0.35rem 0.9rem; background: rgba(15, 23, 42, 0.05); font-size: 0.85rem; color: #1f2937; box-shadow: inset 0 0 0 1px rgba(15,23,42,0.05); }
.method-pill strong { font-weight: 700; font-size: 1rem; }
.method-pill small { font-size: 0.75rem; color: #6b7280; }
.method-label { font-weight: 600; text-transform: uppercase; font-size: 0.75rem; letter-spacing: 0.04em; }
.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

.stat-card { 
  border: none; 
  border-radius: 20px; 
  padding: 1.75rem; 
  color: white; 
  display: flex; 
  align-items: center; 
  gap: 1.25rem; 
  transition: transform 0.3s, box-shadow 0.3s; 
  box-shadow: 0 8px 25px rgba(0,0,0,0.12); 
  height: 100%; 
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0.05) 100%);
  pointer-events: none;
}

.stat-card:hover { 
  transform: translateY(-8px); 
  box-shadow: 0 20px 40px rgba(0,0,0,0.2); 
}

.stat-devices { 
  background: linear-gradient(135deg, #667eea, #764ba2); 
  position: relative;
}

.stat-devices::after {
  content: '';
  position: absolute;
  top: -50%;
  right: -50%;
  width: 100%;
  height: 100%;
  background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
  pointer-events: none;
}

.stat-offline { 
  background: linear-gradient(135deg, #f093fb, #f5576c); 
}

.stat-users { 
  background: linear-gradient(135deg, #4facfe, #00f2fe); 
}

.stat-alerts { 
  background: linear-gradient(135deg, #fa709a, #fee140); 
}

.stat-requests { 
  background: linear-gradient(135deg, #43e97b, #38f9d7); 
}

.stat-unique-users { 
  background: linear-gradient(135deg, #a18cd1, #fbc2eb); 
}

.stat-endpoints { 
  background: linear-gradient(135deg, #fccb90, #d57eeb); 
}

.stat-peak { 
  background: linear-gradient(135deg, #f6d365, #fda085); 
}

.stat-icon { 
  width: 70px; 
  height: 70px; 
  background: rgba(255,255,255,0.25); 
  border-radius: 16px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-size: 1.6rem; 
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255,255,255,0.2);
  position: relative;
  z-index: 1;
}

.stat-content { 
  position: relative;
  z-index: 1;
}

.stat-content h3 { 
  font-size: 2.25rem; 
  font-weight: 700; 
  margin: 0; 
  text-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.stat-content span { 
  opacity: 0.95; 
  font-size: 0.9rem; 
  font-weight: 500;
}

.stat-detail { 
  margin-top: 0.5rem; 
  font-size: 0.8rem; 
}
.stat-detail span {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.2rem 0.65rem;
  border-radius: 999px;
  font-weight: 600;
  letter-spacing: 0.015em;
  background: rgba(255, 255, 255, 0.25);
  color: #ffffff !important;
  text-shadow: 0 1px 2px rgba(15, 23, 42, 0.3);
}
.stat-devices .stat-detail span {
  background: rgba(16, 185, 129, 0.25);
  border: 1px solid rgba(16, 185, 129, 0.4);
  color: #ecfdf5 !important;
}

.chart-card, .data-card { border: none; border-radius: 16px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); height: 100%; display: flex; flex-direction: column; }
.chart-card .card-header, .data-card .card-header { background: transparent; border-bottom: 1px solid #eee; padding: 1rem 1.5rem; }
.chart-card .card-body { padding: 1.5rem; flex: 1 1 auto; }
.data-card .card-body { padding: 1.5rem; flex: 1 1 auto; overflow: visible; }
.compact-chart .card-body { padding: 1rem 1.5rem 1.5rem; }

.user-item { display: flex; align-items: flex-start; gap: 1rem; padding: 0.75rem; background: #f8f9fa; border-radius: 12px; margin-bottom: 0.5rem; cursor: pointer; transition: background 0.2s; }
.user-item:hover { background: #e9ecef; }
.user-avatar { width: 40px; height: 40px; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 0.8rem; flex-shrink: 0; }
.user-info h6 { margin: 0 0 0.25rem 0; font-weight: 600; font-size: 0.875rem; }
.user-stats { display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap; font-size: 0.75rem; }
.user-methods { display: flex; gap: 0.25rem; flex-wrap: wrap; }
.user-mini-card .card-body { padding:1.25rem; }
.user-mini-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 0.75rem; }
.mini-card { background: #f8f9ff; border-radius: 14px; padding: 0.85rem; box-shadow: inset 0 0 0 1px rgba(102,126,234,0.08); cursor: pointer; transition: transform 0.2s, box-shadow 0.2s; }
.mini-card:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(103,118,255,0.15); }
.mini-avatar { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: #fff; font-weight: 600; font-size: 0.85rem; }
.mini-name { font-weight: 600; font-size: 0.9rem; }
.mini-meta { font-size: 0.72rem; color: #6b7280; }
.mini-methods { display: flex; flex-wrap: wrap; gap: 0.35rem; margin-top: 0.5rem; }
.method-chip { font-size: 0.65rem; padding: 0.15rem 0.4rem; border-radius: 999px; background: #eef2ff; color: #4338ca; }

.endpoint-user-item { border-bottom: 1px solid #f1f1f1; padding: 0.75rem 0; }
.endpoint-user-item:last-child { border-bottom: none; }
.endpoint-user-hours { display: flex; flex-wrap: wrap; gap: 0.35rem; margin-top: 0.5rem; }
.hour-chip { font-size: 0.65rem; padding: 0.1rem 0.45rem; border-radius: 999px; background: rgba(102,126,234,0.12); color: #4338ca; }

.method-badge { font-size: 0.65rem; padding: 0.15rem 0.4rem; border-radius: 4px; background: #e9ecef; color: #495057; }
.method-get { background: #e0e7ff; color: #3730a3; }
.method-post { background: #dbeafe; color: #1e40af; }
.method-put { background: #fce7f3; color: #9d174d; }
.method-delete { background: #fee2e2; color: #991b1b; }

.endpoint-item { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid #f0f0f0; }
.endpoint-item:last-child { border-bottom: none; }
.endpoint-path { font-family: monospace; font-size: 0.8rem; color: #374151; }

.device-item { display: flex; align-items: center; gap: 1rem; padding: 0.75rem; background: #f8f9fa; border-radius: 12px; margin-bottom: 0.5rem; }
.device-item.offline { border-left: 4px solid #dc3545; }
.device-icon { width: 35px; height: 35px; background: linear-gradient(135deg, #667eea, #764ba2); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: white; font-size: 0.9rem; }
.device-info h6 { margin: 0 0 0.15rem 0; font-weight: 600; font-size: 0.85rem; }
.device-meta { display: flex; gap: 0.5rem; align-items: center; font-size: 0.7rem; }

.battery-item { display: flex; align-items: center; gap: 1rem; padding: 0.5rem; background: #f8f9fa; border-radius: 10px; margin-bottom: 0.5rem; }
.battery-icon { width: 30px; height: 30px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-size: 1rem; }
.battery-critical { background: #fee2e2; color: #dc3545; }
.battery-low { background: #fef3c7; color: #f59e0b; }
.battery-ok { background: #d1fae5; color: #10b981; }
.battery-info { flex: 1; }
.battery-info h6 { margin: 0; font-size: 0.85rem; }

.company-item { display: flex; align-items: center; gap: 1rem; padding: 0.75rem 0; border-bottom: 1px solid #eee; }
.company-item:last-child { border-bottom: none; }
.company-rank { width: 30px; height: 30px; background: linear-gradient(135deg, #667eea, #764ba2); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; font-size: 0.875rem; }
.company-info { flex: 1; }
.company-info h6 { margin: 0 0 0.25rem 0; font-size: 0.875rem; }

.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1050; }
.modal-panel { background: white; border-radius: 20px; width: min(96vw, 1200px); max-height: 85vh; overflow-y: auto; box-shadow: 0 20px 60px rgba(15, 23, 42, 0.25); }
.modal-panel table { font-size: 0.9rem; }
.modal-panel thead th { white-space: nowrap; }
.modal-panel tbody td { vertical-align: middle; }
.modal-panel .modal-header { padding: 1rem 1.5rem; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
.modal-panel .modal-body { padding: 1.5rem; }
.modal-panel .modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #eee; text-align: right; }

.data-card .card-body::-webkit-scrollbar { width: 6px; }
.data-card .card-body::-webkit-scrollbar-track { background: #f1f1f1; border-radius: 3px; }
.data-card .card-body::-webkit-scrollbar-thumb { background: #c1c1c1; border-radius: 3px; }

@media (max-width: 1200px) {
  .dashboard-main { padding: 1.5rem; }
}

@media (max-width: 992px) {
  .dashboard-controls { justify-content: flex-start; }
  .date-input { width: 160px; }
  .stat-card { padding: 1.25rem; }
  .stat-icon { width: 48px; height: 48px; font-size: 1.2rem; }
}

@media (max-width: 768px) {
  .dashboard { background: linear-gradient(135deg, #667eea 0%, #5f72be 100%); }
  .dashboard-main { padding: 1.25rem; }
  .dashboard-controls { flex-direction: column; align-items: stretch; }
  .date-picker-wrapper, .user-filter-wrapper, .dashboard-refresh { width: 100%; }
  .date-input, .user-filter-wrapper select { width: 100%; min-width: 0; }
  .stat-card { flex-direction: row; align-items: center; }
  .chart-card .card-body, .data-card .card-body { padding: 1rem; }
  .user-mini-grid { grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); }
}

@media (max-width: 576px) {
  .dashboard-main { padding: 1rem 0.75rem 2rem; }
  .stat-card { flex-direction: column; align-items: flex-start; text-align: left; }
  .stat-content h3 { font-size: 1.5rem; }
  .stat-icon { width: 42px; height: 42px; }
  .chart-card .card-header, .data-card .card-header { padding: 0.75rem 1rem; }
  .chart-card .card-body, .data-card .card-body { padding: 0.75rem 1rem 1rem; }
  .modal-panel { width: 98vw; border-radius: 14px; }
}
</style>
