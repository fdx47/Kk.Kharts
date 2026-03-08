import { api } from './api';

// baseURL em api.ts já inclui /api/v1. Aqui só o path relativo a partir de /api/v1
const BASE = '/growflex/mustache/drain-pluviometre';

export function getDrainage(devEui: string, startDate: string, endDate: string, waterUsedLiters = 0) {
  return api.get(BASE, {
    params: { devEui, startDate, endDate, waterUsedLiters },
  });
}

export function postDrainage(devEui: string, payload: { startAt: string; endAt: string; waterUsedLiters: number }) {
  return api.post(BASE, payload, {
    params: { devEui },
  });
}
