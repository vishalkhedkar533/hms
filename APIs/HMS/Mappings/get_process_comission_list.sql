SELECT 
    jeh.job_exe_hist_id AS "JobExeHistId",
    jeh.job_config_id   AS "JobConfigId",
    jc.job_name         AS "CommissionName",
    jeh.started_at      AS "StartedAt",
    jeh.finished_at     AS "FinishedAt",
    jeh.totalrecs       AS "Records",
    jeh.exe_status      AS "ExeStatus",
    jeh.download_lnk    AS "DownloadLnk",
    jeh.orgid           AS "OrgId",
    COUNT(*) OVER()     AS "TotalCount"
FROM scheduler.job_exe_hist jeh
LEFT JOIN scheduler.job_config jc 
       ON jeh.job_config_id = jc.job_config_id
WHERE jeh.orgid = :p_orgid
  AND jc.targettype = 'Tasks.Insurance.Commission'
  AND (
      NULLIF(TRIM(COALESCE(:p_commission_name, '')), '') IS NULL
      OR jc.job_name ILIKE '%' || TRIM(:p_commission_name) || '%'
  )
  AND (:p_from_date IS NULL OR jeh.started_at::date >= :p_from_date::date)
  AND (:p_to_date IS NULL OR jeh.started_at::date <= :p_to_date::date)
ORDER BY jeh.started_at DESC
LIMIT :p_page_size 
OFFSET ((:p_page_number - 1) * :p_page_size);
