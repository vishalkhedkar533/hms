INSERT INTO hmsmaster.organization_periods (start_date, end_date, organization_id, range_type)
SELECT 
    date_trunc('month', month_series)::date AS start_date,
    (date_trunc('month', month_series) + interval '1 month - 1 day')::date AS end_date,
    :p_organization_id AS organization_id,,
    'MonthPeriod' RangeType
FROM 
    generate_series(
        date_trunc('year', now()), 
        date_trunc('year', now()) + interval '11 months', 
        interval '1 month'
    ) AS month_series
ON CONFLICT (organization_id, start_date, end_date) 
DO NOTHING