import { decodeToken } from "./authService.js";

export function extractRolesFromToken() {
  const payload = decodeToken();
  if (!payload) return [];
  const candidates = [];
  if (payload.role !== undefined) candidates.push(payload.role);
  if (payload.roles !== undefined) candidates.push(payload.roles);
  const uri1 = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
  const uri2 = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role";
  if (payload[uri1] !== undefined) candidates.push(payload[uri1]);
  if (payload[uri2] !== undefined) candidates.push(payload[uri2]);
  const roles = [];
  for (const c of candidates) {
    if (Array.isArray(c)) {
      for (const v of c) if (typeof v === "string") roles.push(v);
    } else if (typeof c === "string") {
      c.split(/[;,\s]+/).forEach((s) => {
        if (s) roles.push(s);
      });
    }
  }
  return roles;
}

export function hasRole(roleName) {
  if (!roleName) return false;
  const target = String(roleName).toLowerCase();
  return extractRolesFromToken().some(
    (r) => String(r).trim().toLowerCase() === target,
  );
}

export function isRootUser() {
  return hasRole("Root");
}
