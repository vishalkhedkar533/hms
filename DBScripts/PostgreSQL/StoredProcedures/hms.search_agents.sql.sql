-- DROP FUNCTION hms.search_agents(varchar, varchar, varchar, varchar, varchar, varchar, varchar, int8, int8, varchar, varchar);
CREATE OR REPLACE FUNCTION hms.search_agents (
  p_agent_name character varying DEFAULT NULL::character varying,
  p_email character varying DEFAULT NULL::character varying,
  p_mobileno character varying DEFAULT NULL::character varying,
  p_pan_number character varying DEFAULT NULL::character varying,
  p_aadhaar_number character varying DEFAULT NULL::character varying,
  p_irda_license_number character varying DEFAULT NULL::character varying,
  p_gst_number character varying DEFAULT NULL::character varying,
  p_page_number bigint DEFAULT 1,
  p_page_size bigint DEFAULT 10,
  p_sort_column character varying DEFAULT 'agent_id'::character varying,
  p_sort_direction character varying DEFAULT 'ASC'::character varying
) RETURNS TABLE (
  agent_id integer,
  agent_code character varying,
  agent_name character varying,
  email character varying,
  mobileno character varying,
  pan_number character varying,
  aadhaar_number character varying,
  irda_license_number character varying,
  gst_number character varying,
  total_count integer
) LANGUAGE plpgsql AS $function$
BEGIN
    RETURN QUERY
    WITH filtered AS (
        SELECT a.agent_id::INT,
               a.agent_code::VARCHAR,
               a.agent_name::VARCHAR,
               a.email::VARCHAR,
               a.mobileno::VARCHAR,
               a.pan_number::VARCHAR,
               a.aadhaar_number::VARCHAR,
               a.irda_license_number::VARCHAR,
               a.gst_number::VARCHAR
        FROM hms.agent a
        WHERE (p_agent_name IS NULL OR a.agent_name ILIKE '%' || p_agent_name || '%')
          AND (p_email IS NULL OR a.email ILIKE '%' || p_email || '%')
          AND (p_mobileno IS NULL OR a.mobileno ILIKE '%' || p_mobileno || '%')
          AND (p_pan_number IS NULL OR a.pan_number ILIKE '%' || p_pan_number || '%')
          AND (p_aadhaar_number IS NULL OR a.aadhaar_number ILIKE '%' || p_aadhaar_number || '%')
          AND (p_irda_license_number IS NULL OR a.irda_license_number ILIKE '%' || p_irda_license_number || '%')
          AND (p_gst_number IS NULL OR a.gst_number ILIKE '%' || p_gst_number || '%')
    ),
    counted AS (
        SELECT COUNT(*)::INT AS total_count FROM filtered
    )
    SELECT f.agent_id,
           f.agent_code,
           f.agent_name,
           f.email,
           f.mobileno,
           f.pan_number,
           f.aadhaar_number,
           f.irda_license_number,
           f.gst_number,
           c.total_count
    FROM filtered f
    CROSS JOIN counted c
    ORDER BY
        CASE 
            WHEN p_sort_column = 'agent_id' AND upper(p_sort_direction) = 'ASC' THEN f.agent_id
            ELSE NULL
        END ASC,
        CASE 
            WHEN p_sort_column = 'agent_id' AND upper(p_sort_direction) = 'DESC' THEN f.agent_id
            ELSE NULL
        END DESC,
        CASE 
            WHEN p_sort_column = 'agent_name' AND upper(p_sort_direction) = 'ASC' THEN f.agent_name
            ELSE NULL
        END ASC,
        CASE 
            WHEN p_sort_column = 'agent_name' AND upper(p_sort_direction) = 'DESC' THEN f.agent_name
            ELSE NULL
        END DESC,
        CASE 
            WHEN p_sort_column = 'email' AND upper(p_sort_direction) = 'ASC' THEN f.email
            ELSE NULL
        END ASC,
        CASE 
            WHEN p_sort_column = 'email' AND upper(p_sort_direction) = 'DESC' THEN f.email
            ELSE NULL
        END DESC
    OFFSET ((p_page_number - 1) * p_page_size)::INT
    LIMIT p_page_size::INT;
END;
$function$;