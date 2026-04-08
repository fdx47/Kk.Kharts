// HfSqlForwarder Dashboard JavaScript

// Store start time for uptime calculation
if (!window.dashboardStartTime) {
    window.dashboardStartTime = Date.now();
}

function dashboard() {
    return {
        authenticated: false,
        loginForm: { secret: '' },
        loginError: '',
        state: { hfsqlHealthy: false, lastCycle: '-', lastCycleStatus: '', lastError: '', lastHealth: '-', lastHealthTime: null },
        stateDump: {},
        config: { numelt: null, intervalMinutes: null },
        configStatus: null,
        logs: [],
        records: { today: 0, lastRead: null, table: '' },
        showLogs: true,  // Mudar para true para mostrar por padrão
        showValues: false,
        tableData: { table: '', count: 0, data: [], hasMore: false },
        loadDataStatus: 'pending',
        loadDataError: '',
        testDatabaseStatus: null,
        databaseResult: null,
        systemLogs: [],
        systemLogsStatus: 'pending',
        systemLogsError: '',
        activeLogTab: 'app',
        uptime: '0h 0m 0s',
        
        init() {
            // Garantir que o tempo de início seja definido
            if (!window.dashboardStartTime) {
                window.dashboardStartTime = Date.now();
                console.log('Dashboard start time initialized:', new Date(window.dashboardStartTime));
            }
            
            const saved = localStorage.getItem('hf-admin-secret');
            if (saved) { 
                this.loginForm.secret = saved; 
                this.authenticated = true; 
                this.refreshAll(); 
            }
            this.refreshAll();
            
            // Atualizar uptime a cada segundo
            setInterval(() => {
                this.uptime = this.getUptime();
            }, 1000);
            
            // Atualizar imediatamente
            this.uptime = this.getUptime();
        },
        
        prefill(val) { 
            this.loginForm.secret = val; 
        },
        
        logout() { 
            this.authenticated = false; 
            localStorage.removeItem('hf-admin-secret'); 
            this.loginForm.secret = ''; 
        },
        
        async login() {
            if (!this.loginForm.secret) { 
                this.loginError = 'Secret requis'; 
                return; 
            }
            localStorage.setItem('hf-admin-secret', this.loginForm.secret);
            this.loginError = ''; 
            this.authenticated = true; 
            await this.refreshAll();
        },
        
        headers() { 
            return { 
                'X-Admin-Secret': this.loginForm.secret || localStorage.getItem('hf-admin-secret') || '', 
                'Content-Type': 'application/json' 
            }; 
        },
        
        async call(path, method='GET', body=null) {
            try {
                const res = await fetch(`/api/admin${path}`, { 
                    method, 
                    headers: this.headers(), 
                    body: body ? JSON.stringify(body) : null 
                });
                const text = await res.text();
                let data = text; 
                try { data = JSON.parse(text); } catch {}
                return { ok: res.ok, data };
            } catch (error) {
                console.error('API call failed:', error);
                return { ok: false, data: null, error };
            }
        },
        
        async loadConfig() {
            const r = await this.call('/config');
            if (r.ok) { 
                this.config.numelt = r.data?.filtre?.numElt ?? null; 
                this.config.intervalMinutes = r.data?.scheduler?.intervalMinutes ?? null; 
                this.configStatus = null; 
            }
        },
        
        async saveConfig() {
            const payload = { NumElt: this.config.numelt ?? 0, IntervalMinutes: this.config.intervalMinutes ?? 3 };
            const r = await this.call('/config', 'POST', payload);
            this.configStatus = r.ok ? 'ok' : 'err';
        },
        
        async loadHealth() {
            const r = await this.call('/health');
            if (r.ok) {
                this.state.hfsqlHealthy = r.data?.status === 'OK';
                this.state.lastHealth = r.data?.status ?? 'N/A';
                this.state.lastHealthTime = r.data?.lastRunUtc ?? null;
            }
            this.stateDump.health = r.data;
        },
        
        async loadState() {
            const r = await this.call('/state');
            if (r.ok) {
                const stateData = r.data?.state ?? {};
                this.state.lastCycle = stateData?.lastCycle ?? '-';
                this.state.lastCycleStatus = stateData?.lastStatus ?? '';
                this.state.lastError = stateData?.lastError ?? '';
            }
            this.stateDump.state = r.data;
        },
        
        async refreshLogs() {
            const r = await this.call('/logs');
            if (Array.isArray(r.data)) this.logs = r.data;
        },
        
        async loadRecords() {
            const r = await this.call('/records');
            if (r.ok) {
                this.records = r.data;
            }
        },
        
        async loadData() {
            try {
                const r = await this.call('/data');
                if (r.ok) {
                    this.tableData = r.data;
                    this.loadDataStatus = r.data.count > 0 ? 'success' : 'empty';
                } else {
                    this.loadDataStatus = 'error';
                    this.loadDataError = r.error || 'Erreur de chargement';
                }
            } catch (error) {
                console.error('Error loading table data:', error);
                this.loadDataStatus = 'error';
                this.loadDataError = 'Erreur de connexion';
            }
        },
        
        async loadSystemLogs() {
            try {
                const r = await this.call('/system-logs');
                if (r.ok && Array.isArray(r.data)) {
                    this.systemLogs = r.data;
                    this.systemLogsStatus = 'success';
                } else {
                    this.systemLogsStatus = 'error';
                    this.systemLogsError = 'Erreur de chargement des logs système';
                }
            } catch (error) {
                console.error('Error loading system logs:', error);
                this.systemLogsStatus = 'error';
                this.systemLogsError = 'Erreur de connexion';
            }
        },
        
        clearLogsView() { 
            this.logs = []; 
        },
        
        async refreshAll() {
            await this.loadConfig();
            await this.loadHealth();
            await this.loadState();
            await this.loadRecords();
            await this.loadData();
            await this.loadSystemLogs();
            await this.refreshLogs();
        },
        
        formatDate(ts) { 
            if (!ts) return '-'; 
            return new Date(ts).toLocaleString(); 
        },
        
        formatDateTime(ts) { 
            if (!ts) return '-'; 
            return new Date(ts).toLocaleString(); 
        },
        
        formatTime(time) {
            if (!time) return '-';
            const hours = Math.floor(time / 3600);
            const minutes = Math.floor((time % 3600) / 60);
            const seconds = time % 60;
            return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        },
        
        formatMinutes(minutes) {
            if (!minutes) return '-';
            const hours = Math.floor(minutes / 60);
            const mins = minutes % 60;
            return `${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}`;
        },
        
        json(obj) { 
            return JSON.stringify(obj ?? {}, null, 2); 
        },
        
        levelClass(level) {
            const map = { 
                INF: 'bg-green-500/20 text-green-200', 
                WRN: 'bg-amber-500/20 text-amber-200', 
                ERR: 'bg-red-500/20 text-red-200' 
            };
            return (map[level] || 'bg-white/10 text-slate-200') + ' pill px-2';
        },
        
        getUptime() {
            const startTime = window.dashboardStartTime || Date.now();
            const uptime = Date.now() - startTime;
            
            // Debug
            if (uptime < 1000) {
                console.log('Uptime debug - startTime:', startTime, 'now:', Date.now(), 'uptime:', uptime);
            }
            
            const hours = Math.floor(uptime / (1000 * 60 * 60));
            const minutes = Math.floor((uptime % (1000 * 60 * 60)) / (1000 * 60));
            const seconds = Math.floor((uptime % (1000 * 60)) / 1000);
            
            return `${hours}h ${minutes}m ${seconds}s`;
        },
        
        async testDatabase() {
            try {
                const r = await this.call('/health');
                if (r.ok) {
                    this.showDatabaseResult('success', 'Connexion HFSQL établie!');
                } else {
                    this.showDatabaseResult('error', 'Erreur connexion HFSQL');
                }
            } catch (error) {
                this.showDatabaseResult('error', 'Erreur de test: ' + error.message);
            }
        },
        
        async testDatabaseWithIcon() {
            this.testDatabaseStatus = 'testing';
            try {
                const r = await this.call('/health');
                if (r.ok) {
                    this.showDatabaseResult('success', 'Connexion HFSQL établie!');
                    this.testDatabaseStatus = 'success';
                } else {
                    this.showDatabaseResult('error', 'Erreur connexion HFSQL');
                    this.testDatabaseStatus = 'error';
                }
            } catch (error) {
                this.showDatabaseResult('error', 'Erreur de test: ' + error.message);
                this.testDatabaseStatus = 'error';
            } finally {
                setTimeout(() => {
                    this.testDatabaseStatus = null;
                }, 3000);
            }
        },
        
        showDatabaseResult(status, message) {
            this.databaseResult = { status, message };
            setTimeout(() => {
                this.databaseResult = null;
            }, 5000);
        },
        
        async refreshDatabase() {
            await this.loadHealth();
            await this.loadState();
        }
    }
}
