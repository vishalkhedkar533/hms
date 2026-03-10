INSERT INTO hmsmaster.organization_periods (start_date, end_date, orgid, range_type)
SELECT 
    quarter_series::date AS start_date,
    (quarter_series + interval '3 months - 1 day')::date AS end_date,
    :p_organization_id AS orgid,
    'QuarterPeriod' AS range_type
FROM 
    generate_series(
        (date_trunc('year', now()) + interval '3 months'), -- Starts April 1st
        (date_trunc('year', now()) + interval '12 months'), -- Ends Jan 1st (next yr)
        interval '3 months'                                -- Jumps by quarters
    ) AS quarter_series
ON CONFLICT (orgid, start_date, end_date) 
DO NOTHING