-- DROP FUNCTION hms.get_agent_hierarchy_json(int4);

CREATE OR REPLACE FUNCTION hms.get_agent_supervisors(p_agent_id integer)
 RETURNS jsonb
 LANGUAGE sql
AS $function$
WITH RECURSIVE hierarchy AS (
    -- Step 1: get all paths containing the target agent
    SELECT
        ah.hierarchy_path,
        string_to_array(ah.hierarchy_path::TEXT, '.') AS labels
    FROM hms.agent_hierarchy ah
    WHERE ah.hierarchy_path ~ ('*.' || p_agent_id::TEXT || '*')::lquery
),
json_tree AS (
    -- Step 2: start from the deepest node (agent) and build supervisors recursively
    SELECT
        array_length(labels, 1) AS lvl,
        labels[array_length(labels,1)]::INT AS agent_id,
        jsonb_build_object(
            'AgentId', a.agent_id,
            'FirstName', a.first_name,
            'MiddleName', a.middle_name,
            'LastName', a.last_name,
            'AgentCode', a.agent_code,
            'supervisors', NULL
        ) AS node,
        labels
    FROM hierarchy h
    JOIN hms.agent a ON a.agent_id = labels[array_length(labels,1)]::INT

    UNION ALL

    SELECT
        j.lvl - 1,
        labels[j.lvl - 1]::INT AS agent_id,
        jsonb_build_object(
            'AgentId', a.agent_id,
            'FirstName', a.first_name,
            'MiddleName', a.middle_name,
            'LastName', a.last_name,
            'AgentCode', a.agent_code,
            'supervisors', j.node
        ) AS node,
        labels
    FROM json_tree j
    JOIN hms.agent a ON a.agent_id = labels[j.lvl - 1]::INT
    WHERE j.lvl > 1
)
SELECT jsonb_agg(node)
FROM json_tree
WHERE lvl = 1;
$function$
;