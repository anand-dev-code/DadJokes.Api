BEGIN;

CREATE OR REPLACE FUNCTION sp_save_jokes(p_term text, p_jokes jsonb)
RETURNS void AS $$
DECLARE
  rec jsonb;
  v_id uuid;
  v_text text;
  v_source varchar;
  v_word_count int;
  v_created_at timestamptz;
  v_group_type varchar;
BEGIN
  -- iterate over array
  FOR rec IN SELECT * FROM jsonb_array_elements(p_jokes)
  LOOP
    -- extract fields with safe fallbacks
    v_id := CASE WHEN rec ? 'id' THEN (rec->>'id')::uuid ELSE uuid_generate_v4() END;
    v_text := COALESCE(rec->>'jokeText', rec->>'joke_text', '');
    v_source := COALESCE(rec->>'source', 'External');
    v_word_count := COALESCE(NULLIF(rec->>'wordCount', '' )::int, 0);
    IF v_word_count = 0 THEN
      -- fallback: compute word count by simple whitespace split if not provided
      v_word_count := array_length(regexp_split_to_array(regexp_replace(v_text, '\s+', ' ', 'g'), ' '), 1);
      IF v_word_count IS NULL THEN v_word_count := 0; END IF;
    END IF;
    v_created_at := COALESCE((rec->>'createdAt')::timestamptz, now());

    -- determine group type
    v_group_type := CASE WHEN v_word_count < 10 THEN 'Short'
                         WHEN v_word_count < 20 THEN 'Medium'
                         ELSE 'Long' END;

    -- upsert into jokes
    INSERT INTO jokes (id, joke_text, source, word_count, created_at)
    VALUES (v_id, v_text, v_source, v_word_count, v_created_at)
    ON CONFLICT (id) DO UPDATE
      SET joke_text = EXCLUDED.joke_text,
          source = EXCLUDED.source,
          word_count = EXCLUDED.word_count,
          created_at = EXCLUDED.created_at;

    -- insert saved_groups association (idempotent by unique constraint if you add one)
    INSERT INTO saved_groups (id, term, group_type, joke_id, saved_at)
    VALUES (uuid_generate_v4(), p_term, v_group_type, v_id, now())
    ON CONFLICT DO NOTHING;
  END LOOP;
END;
$$ LANGUAGE plpgsql;
COMMIT;