INSERT INTO hmsmaster.organization_periods (start_date, end_date, orgid, range_type)
SELECT 
    date_trunc('month', month_series)::date AS start_date,
    (date_trunc('month', month_series) + interval '1 month - 1 day')::date AS end_date,
    :p_organization_id AS orgid,
    'MonthPeriod' RangeType
FROM 
    generate_series(
        date_trunc('year', now()), 
        date_trunc('year', now()) + interval '11 months', 
        interval '1 month'
    ) AS month_series
ON CONFLICT (orgid, start_date, end_date) 
DO NOTHING