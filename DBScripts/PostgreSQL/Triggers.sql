CREATE OR REPLACE FUNCTION hms."log_agent_movement_history_changes"()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        INSERT INTO hms."AGENT_MOVEMENT_HISTORY_AUDIT"
        SELECT NEW.*, 'INSERT', current_user, now();
        RETURN NEW;

    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO hms."AGENT_MOVEMENT_HISTORY_AUDIT"
        SELECT OLD.*, 'UPDATE', current_user, now();
        RETURN NEW;

    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO hms."AGENT_MOVEMENT_HISTORY_AUDIT"
        SELECT OLD.*, 'DELETE', current_user, now();
        RETURN OLD;
    END IF;

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;
