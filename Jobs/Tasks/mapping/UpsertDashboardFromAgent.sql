WITH agg AS (
    SELECT
        a.orgid::bigint AS orgid,
        a.channel::bigint AS channel_id,
        a.subchannel::bigint AS subchannel_id,
        COUNT(*)::float8 AS totalentitiescount,
        COUNT(*) FILTER (WHERE DATE_TRUNC('month', a.created_date) = DATE_TRUNC('month', NOW()))::float8 AS totalentitiesthismonth,
        COUNT(*) FILTER (WHERE DATE_TRUNC('month', a.created_date) = DATE_TRUNC('month', NOW()))::float8 AS entitiescreatedthismonth,
        COUNT(*) FILTER (WHERE DATE_TRUNC('month', a.created_date) = DATE_TRUNC('month', NOW()) - INTERVAL '1 month')::float8 AS entitiescreatedprevmonth,
        COUNT(*) FILTER (
            WHERE LOWER(COALESCE(a.agent_status_code, '')) = 'terminated'
              AND DATE_TRUNC('month', COALESCE(a.modified_date, a.created_date)) = DATE_TRUNC('month', NOW())
        )::float8 AS entitiesterminatedthismonth,
        COUNT(*) FILTER (
            WHERE LOWER(COALESCE(a.agent_status_code, '')) = 'terminated'
              AND DATE_TRUNC('month', COALESCE(a.modified_date, a.created_date)) = DATE_TRUNC('month', NOW()) - INTERVAL '1 month'
        )::float8 AS entitiesterminatedprevmonth,
        (
            COUNT(*) FILTER (WHERE DATE_TRUNC('month', a.created_date) = DATE_TRUNC('month', NOW()))
            - COUNT(*) FILTER (
                WHERE LOWER(COALESCE(a.agent_status_code, '')) = 'terminated'
                  AND DATE_TRUNC('month', COALESCE(a.modified_date, a.created_date)) = DATE_TRUNC('month', NOW())
            )
        )::float8 AS entitiesnetthismonth,
        COUNT(*) FILTER (
            WHERE a.licenseexpirydate IS NOT NULL
              AND a.licenseexpirydate::date <= (CURRENT_DATE + INTERVAL '30 months')::date
        )::float8 AS licenseexpiringin30months,
        0::float8 AS certificateexpiringin30months,
        0::float8 AS mbgcriterianotmet
    FROM hms.agent a
    WHERE a.orgid = @OrgId
    GROUP BY a.orgid, a.channel, a.subchannel
),
upd AS (
    UPDATE hms.hms_dashboard d
    SET
        totalentitiescount = a.totalentitiescount,
        totalentitiesthismonth = a.totalentitiesthismonth,
        entitiescreatedthismonth = a.entitiescreatedthismonth,
        entitiescreatedprevmonth = a.entitiescreatedprevmonth,
        entitiesterminatedthismonth = a.entitiesterminatedthismonth,
        entitiesterminatedprevmonth = a.entitiesterminatedprevmonth,
        entitiesnetthismonth = a.entitiesnetthismonth,
        licenseexpiringin30months = a.licenseexpiringin30months,
        certificateexpiringin30months = a.certificateexpiringin30months,
        mbgcriterianotmet = a.mbgcriterianotmet
    FROM agg a
    WHERE d.orgid = a.orgid
      AND COALESCE(d.channel_id, -1) = COALESCE(a.channel_id, -1)
      AND COALESCE(d.subchannel_id, -1) = COALESCE(a.subchannel_id, -1)
    RETURNING d.dashboard_id
)
INSERT INTO hms.hms_dashboard (
    orgid, channel_id, subchannel_id, totalentitiescount, totalentitiesthismonth, entitiescreatedthismonth,
    entitiescreatedprevmonth, entitiesterminatedthismonth, entitiesterminatedprevmonth, entitiesnetthismonth,
    licenseexpiringin30months, certificateexpiringin30months, mbgcriterianotmet
)
SELECT
    a.orgid, a.channel_id, a.subchannel_id, a.totalentitiescount, a.totalentitiesthismonth, a.entitiescreatedthismonth,
    a.entitiescreatedprevmonth, a.entitiesterminatedthismonth, a.entitiesterminatedprevmonth, a.entitiesnetthismonth,
    a.licenseexpiringin30months, a.certificateexpiringin30months, a.mbgcriterianotmet
FROM agg a
WHERE NOT EXISTS (
    SELECT 1
    FROM hms.hms_dashboard d
    WHERE d.orgid = a.orgid
      AND COALESCE(d.channel_id, -1) = COALESCE(a.channel_id, -1)
      AND COALESCE(d.subchannel_id, -1) = COALESCE(a.subchannel_id, -1)
);