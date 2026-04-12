# MVP Scope — Online Healthcare Platform (Zimbabwe-first)

This document narrows the full backlog in `tasks.md` to a **first shippable product**: a working end-to-end path for patients and doctors in Zimbabwe, with room to grow. Treat everything here as **planning defaults**; product and legal sign-off may adjust priorities.

---

## MVP goal (what “working” means)

A **patient** can register, find a doctor, book a **virtual** appointment, **pay**, join a **telemedicine** session, and receive a **digital prescription** stored in their health record. A **doctor** can register (pending verification), manage availability, conduct the session, and issue a prescription. An **admin** can **verify doctor licences** so doctors can operate as active. **Notifications** (at least email + one mobile channel strategy) confirm bookings and critical events. **Audit logging** applies to security-sensitive actions.

Anything not listed in [MVP must ship](#mvp-must-ship) is **post-MVP** unless you explicitly pull it forward.

---

## Zimbabwe deployment assumptions (fixable now)

These are **engineering and product defaults**, not legal advice. **Local counsel** should validate licensing, telemedicine, advertising, and health-data rules before public launch.

| Topic | MVP assumption |
|--------|----------------|
| **Regulator alignment** | Doctor identifiers and verification workflows should align with **Health Professions Council of Zimbabwe (HPCZ)**-style licence numbers and admin verification; exact fields and APIs are implementation details pending stakeholder input. |
| **Data protection** | Apply the same technical controls described in `design.md` (encryption, audit, minimisation). Add a **deployment compliance profile** (e.g. `ZW-MVP`) in configuration for retention and DPA wording — legal text TBD. |
| **Currency** | Use **USD** as the **primary settlement/display currency** for MVP (common for private telehealth); store amounts in minor units; design **multi-currency** as config so **ZiG** (or others) can be added without schema rewrites. |
| **Payments** | Ship **one** production gateway first to reduce integration risk. **Default recommendation:** **Flutterwave** (cards and regional mobile money rails where enabled for your merchant account). **Paystack** or **Stripe** remain optional second connectors. **Direct EcoCash** integration is **post-MVP** unless your chosen gateway exposes it — avoid custom wallet integrations for MVP. |
| **SMS / voice** | Use **one** provider (e.g. **Africa’s Talking** or **Twilio**) via the existing notification abstraction; register sender IDs / templates per carrier rules in Zimbabwe. MVP may use **SMS for OTP and critical alerts** and **email** for non-critical if cost requires. |
| **Emergency dispatch** | **No** live PSAP/dispatch integration in MVP. Manual SOS (task 36.x) remains **scaffold or defer**; next-of-kin alerts can be in scope only if you complete task 15.x with test coverage. Prefer shipping **doctor-triggered emergency contact** after core consult flows are stable. |
| **Insurance & credit** | **Out of MVP:** insurer APIs, credit lines, instalment plans (see task 12.2–12.5, 12.3). Patients pay **pay-as-you-go** only. |
| **AI services** | **Out of MVP:** Python symptom checker, credit scoring, analytics (task 31). Optional **exception:** a minimal **keyword crisis hint** in-app is acceptable only if it reuses a tiny rules-based check in .NET — do not block MVP on `HealthPlatform.AI`. |

---

## MVP must ship (task map)

Complete these **task groups** in order, **subset** where noted.

| Phase | Tasks | Notes |
|-------|--------|--------|
| **Foundation** | **1 – 2** | Monorepo scaffold, Docker Compose, Hangfire, Serilog, OTel, rate limits, health checks, shared envelopes, RBAC skeleton, idempotency middleware. |
| **Auth** | **3** (+ checkpoint **4**) | JWT, roles, MFA for doctor/pharmacy/admin, lockout (P31), new-device detection as time permits — **MFA enforced for doctors** before production. |
| **Identity** | **5** (partial) | Patient + doctor registration and profiles; **pharmacy** can be stubbed or deferred if unused in MVP. Doctor **pending → verified** path must exist. Properties **P1–P5** where applicable. |
| **Search** | **6** | Doctor search with geo proximity (P6); pharmacy/lab search **optional** if not in MVP UI. |
| **Appointments** | **7** | Booking, Redis slot hold (P7), confirmation, reminders, cancellation rules (P8, P9). Physical clinic extras (task 7.10) can follow virtual-first. |
| **Telemedicine** | **8** | Session lifecycle, RTC tokens, recording consent (P10), SignalR basics; polish reconnect (8.5) as stretch. |
| **Payments** | **12** (subset) | **12.1** with **one** gateway + webhooks + idempotency; **12.6** receipt + `PaymentCompleted`; **12.8** failure handling (P17). **Defer:** 12.2, 12.3, 12.5, 12.4–12.5 property tests tied to credit/instalments. **Include** P16 when payment receipt is implemented. |
| **Prescriptions** | **10** | Issue, default expiry (P12), guard for later pharmacy (P11); drug interaction (P21) strongly recommended before wide rollout. |
| **Health records** | **16** (minimal) | Linked record, Mongo entries for consult/prescription; access control (P24) for doctor reads. |
| **Notifications** | **17** (minimal) | Domain-event driven delivery, preferences, log (P29), critical SMS fallback (P30) for defined critical types. |
| **Admin** | **29** (subset) | **29.3** licence verification queue and outcomes; minimal config if needed for fees/policies. Defer disputes, full analytics exports if needed. |

**Post-MVP (do not block first Zimbabwe pilot):** **11** pharmacy orders, **14** adherence, **15** next-of-kin (unless you need emergency SMS early), **19** labs, **30–32** dashboards & AI, **33–36** specialty/IoT/emergency, **34** i18n beyond English, **37–38** full client surfaces (ship **thin** patient + doctor flows first).

---

## Client applications (MVP realism)

- **Minimum:** one **patient** app surface (Flutter or web) and one **doctor** surface that can complete booking → pay → session → prescription.
- **Next.js** dashboards: **admin verification** first; full doctor/pharmacy dashboards match task **38** but can lag API work.

Track thin UI slices against API readiness; avoid blocking backend integration on full task **37–38** feature lists.

---

## Correctness properties in MVP

Prioritise properties that guard **money, access, and safety**:

- **Must:** P1–P10, P11–P12, P16–P17, P24, P29–P31 (where features are enabled).
- **Defer** properties for modules not shipped (e.g. P13 if no pharmacy MVP).

---

## Definition of Done (MVP pilot)

- [ ] Happy path runs in **staging** with real Postgres/Redis/Elasticsearch (Docker or cloud).
- [ ] At least **one** payment provider in **sandbox + production** config toggles.
- [ ] **No PHI** in plaintext logs; audit entries for access denial and sensitive actions.
- [ ] **Runbook** one-pager: deploy, rollback, gateway webhook rotation, on-call for notification failures.

---

## How this relates to `tasks.md`

- `tasks.md` remains the **full** backlog.
- This file is the **MVP filter**. When in doubt, ship the smallest vertical slice that satisfies the MVP goal, then iterate.
