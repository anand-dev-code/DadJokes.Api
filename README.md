# Jokes API

A simple API service to **search, save, and manage jokes** efficiently.

---

## Overview

The Jokes API finds jokes based on a search term.
It first checks for saved (curated) jokes in the database, and if not enough are found, it fetches more from an external jokes provider and stores them for future searches.

---

## Key Features

1. Organized under schema **`jokes_app`**.
2. Two main tables:

   * **`jokes`** – stores joke text and metadata (id, source, word count, created time).
   * **`saved_groups`** – links a joke to a search term.
3. Avoids duplicates using the provider’s **`external_id`**.
4. Jokes are grouped by length:

   * Short: <10 words
   * Medium: 10–19 words
   * Long: 20+ words
5. A stored procedure **`sp_save_jokes`** handles inserts, updates, and linking atomically.
6. Admins can add jokes manually using `/api/admin/jokes/custom`.
7. Ensures fast, predictable searches by caching results.
8. Supports idempotent behavior — repeated saves or deletions don’t cause duplicates.

---

## How a Search Works

1. Check saved jokes for the term.
2. If insufficient, fetch external jokes.
3. Save only needed jokes (based on length groups).
4. Upsert and link via `sp_save_jokes`.
5. Return results grouped by Short, Medium, and Long.

---

## Example: Add Custom Jokes

```json
POST /api/admin/jokes/custom
{
  "term": "what",
  "jokes": [
    { "jokeText": "What do you call fake spaghetti? An impasta.", "source": "Local" },
    { "jokeText": "What did the ocean say to the beach? Nothing — it just waved.", "source": "Local" }
  ]
}
```
## Swagger Screenshot
<img width="1907" height="879" alt="image" src="https://github.com/user-attachments/assets/2bf9907c-592a-4b1f-8f98-161f12019d5a" />

---

## Database Summary

* **Schema:** `jokes_app`
* **Tables:**

  * `jokes (id, external_id, joke_text, source, word_count, created_at)`
  * `saved_groups (id, term, group_type, joke_id, saved_at)`
* **Stored Proc:** `sp_save_jokes(term, jsonb_jokes)`

> “Our API stores curated jokes per term for speed. It deduplicates using provider IDs and uses a single stored procedure for atomic saves — ensuring idempotent, predictable results.”
