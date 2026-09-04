# CV Management System — Course Project Checklist

Stack: ASP.NET Core Web API + EF Core + PostgreSQL (Render) · Angular + PrimeNG +
Tailwind (tailwindcss-primeui plugin) · Render deployment (native auto-deploy on
push).

---

## Phase 0 — Foundation

- [ ] Solution structure (API project, maybe a shared DTOs/contracts project)
- [ ] EF Core + Npgsql configured, first migration applied
- [ ] Render: Postgres instance provisioned, API deployed
- [ ] Angular app scaffolded, PrimeNG + Tailwind (tailwindcss-primeui) wired in
- [ ] Deployed "Hello, world" end-to-end (Angular calls API calls DB) — **always keep this deployable from here on**

## Phase 1 — Auth & Roles

- [ ] ASP.NET Core Identity set up
- [ ] Roles: Candidate, Recruiter, Administrator
- [ ] Social login — minimum 2 providers (e.g. Google + Facebook/GitHub)
- [ ] Non-authenticated users can: register, sign in, browse positions read-only, view public stats
- [ ] Non-authenticated users blocked from: creating/editing positions, CVs, comments, likes, personal pages
- [ ] Admins can drop their own Admin role

## Phase 2 — Attribute Library (Killer Feature #1)

- [ ] `Attribute` entity: Category, unique Name, Description, DataType
- [ ] Attribute types: String, Text (Markdown), Image, Numeric, Date, Period, Boolean, One-of-many
- [ ] Recruiters: create/edit/delete attributes
- [ ] Attribute selection UI: prefix lookup, recently-used, category filter
- [ ] Optimistic concurrency (Version) on Attribute edits

## Phase 3 — Personal Profile

- [ ] Profile page with 4 sections: Me / Info / Projects / CVs
- [ ] **Me**: built-in undeletable attributes (First Name, Last Name, Location, Photo) — same "engine" as library attributes
- [ ] **Info**: candidate adds/removes attributes from library, fills values
- [ ] **Projects**: CRUD, Markdown description, date range, tech tags with autocomplete (reusable tag component)
- [ ] Only owner + Admin can view/edit full profile; Recruiters see CV data read-only only
- [ ] **Auto-save**: local change tracking, save every 5–10s (not per keystroke)
- [ ] Auto-save uses optimistic locking, client handles version conflicts gracefully (reload + message)

## Phase 4 — Positions (Killer Feature #2)

- [ ] Position: Title, Short Description, (optional) Company/Level
- [ ] Public vs restricted (access rule filters) visibility
- [ ] Recruiters: create blank, duplicate, edit, delete — no ownership concept, shared pool
- [ ] Attributes selected from library, ordered
- [ ] Access rule filters (operators depend on attribute type, e.g. `> `, `=`, `checked`)
- [ ] Project tag filters + max project count for generated CV
- [ ] Position's CV list (visible to Recruiter/Admin only)
- [ ] Optimistic locking on Position edits
- [ ] Losing access hides (not deletes) existing CVs

## Phase 5 — CV Generation (Killer Feature #3)

- [ ] CV row is minimal: Id, PositionId, CandidateId, Status, Version (no stored content)
- [ ] Content computed live: profile attributes ∩ position's required attributes + filtered/capped projects
- [ ] Missing attributes auto-added (empty) on CV creation if position requires them
- [ ] In-place editing of an attribute value in a CV → writes through to the profile value (single source of truth)
- [ ] Empty required values highlighted in red
- [ ] Draft → Published workflow; Publish only enabled when all attributes filled
- [ ] Recruiters: read-only rendered view only (cannot edit); Admin can edit
- [ ] At most one CV per Candidate per Position

## Phase 6 — Discussions

- [ ] Discussion tab per Position: author, timestamp, Markdown content
- [ ] Recruiter view: author name links to candidate's public profile
- [ ] Append-only, chronological order
- [ ] Live updates to all active viewers within 2–5s (SignalR recommended)

## Phase 7 — Likes

- [ ] Only Recruiters can like a CV
- [ ] One like per Recruiter per CV, removable
- [ ] Like count shown in CV lists and search results

## Phase 8 — Main Page & Cross-cutting UI

- [ ] Main page: Latest Positions, Top 5 Most Popular Positions, Tag Cloud, Stats
- [ ] Full-text search accessible from every page's top header
- [ ] Consistent, convenient navigation
- [ ] **Table views only** for Positions and CVs — no gallery/tiles (−20% penalty if violated)
- [ ] **No per-row action buttons** — toolbar or contextual "appearing" actions instead (−20% penalty if violated)
- [ ] i18n: English + one more language, persisted choice, UI-only (not user content)
- [ ] Light/dark theme, persisted choice
- [ ] Responsive design incl. mobile
- [ ] No raw `SELECT *` full scans, no queries inside loops, no images stored in DB/server (external cloud storage)

---

## Optional (only after ALL of the above is done)

- [ ] PDF export with QR code linking back to the app
- [ ] Form auth w/ email confirmation (alt. to social login)
- [ ] Badges/achievements system + downloadable SVG panel
- [ ] Field "tuning" options (length limits, regex validators, numeric ranges)
- [ ] CSV/Excel export of CVs for a given position
