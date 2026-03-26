WITH ledger_txn AS (
    SELECT cl.orgid,
           cl.agent_id,
           cl.job_exe_hist_id,
           SUM(COALESCE(cl.trans_amt, 0)) AS trans_amt_total,
           MIN(cl.finperiodfrom) AS finperiodfrom,
           MAX(cl.finperiodto) AS finperiodto
    FROM comss.comms_ledger cl
    GROUP BY cl.orgid, cl.agent_id, cl.job_exe_hist_id
),
ledger_last AS (
    SELECT DISTINCT ON (cl.orgid, cl.agent_id, cl.job_exe_hist_id)
           cl.orgid,
           cl.agent_id,
           cl.job_exe_hist_id,
           cl.entrydate,
           cl.bal_comm_amt
    FROM comss.comms_ledger cl
    ORDER BY cl.orgid,
             cl.agent_id,
             cl.job_exe_hist_id,
             cl.entrydate DESC NULLS LAST,
             cl.agent_period_comms_id DESC
),
fy_last AS (
    SELECT DISTINCT ON (fl.orgid, fl.agent_id)
           fl.orgid,
           fl.agent_id,
           fl.entrydate,
           fl.finperiodfrom,
           fl.finperiodto,
           fl.bal_comm_amt
    FROM comss.comms_fy_ledger fl
    ORDER BY fl.orgid,
             fl.agent_id,
             fl.entrydate DESC NULLS LAST,
             fl.fy_period_comms_id DESC
)
SELECT cjed.comm_job_exe_dtls_id AS "CommJobExeDtlsId",
       cjed.job_exe_hist_id AS "JobExeHistId",
       cjed.agent_id AS "AgentId",
       a.agent_name AS "AgentName",
       cm.channel_name AS "Channel",
       ll.entrydate AS "LedgerEntryDate",
       lt.finperiodfrom AS "LedgerFinPeriodFrom",
       lt.finperiodto AS "LedgerFinPeriodTo",
       lt.trans_amt_total AS "LedgerTransAmountTotal",
       ll.bal_comm_amt AS "LedgerBalanceAmount",
       fl.entrydate AS "FyLedgerEntryDate",
       fl.finperiodfrom AS "FyFinPeriodFrom",
       fl.finperiodto AS "FyFinPeriodTo",
       fl.bal_comm_amt AS "FyBalanceAmount",
       cjed.premiucollid AS "PremiumCollectionId",
       cjed.premium_amt AS "PremiumAmount",
       cjed.formula AS "Formula",
       cjed.comm_amt AS "CommissionAmount",
       cjed.status AS "Status",
       cjed.logs AS "Logs",
       pc.premiucollid AS "PcPremiumCollectionId",
       pc.policyref AS "PolicyRef",
       pc.premium_received_dt AS "PremiumReceivedDate",
       pc.premium_type AS "PremiumType",
       pc.prem_coll_yr AS "PremiumCollectionYear",
       pc.prem_coll_qtr AS "PremiumCollectionQuarter",
       pc.prem_coll_fin_yr AS "PremiumCollectionFinYear",
       ip.policyno AS "PolicyNo",
       ip.policysuffix AS "PolicySuffix",
       ip.riskstartdt AS "RiskStartDate",
       ip.riskenddt AS "RiskEndDate",
       ip.policyterm AS "PolicyTerm",
       ip.prempayingterm AS "PremiumPayingTerm",
       ip.proposerclientid AS "ProposerClientId",
       ip.lifeinsuredclientid AS "LifeInsuredClientId",
       ip.isstaffpolicy AS "IsStaffPolicy",
       ip.policysourcecode AS "PolicySourceCode",
       ip.insuredpan AS "InsuredPAN",
       ip.proposerpan AS "ProposerPAN",
       ip.insureddob AS "InsuredDOB",
       ip.proposerdob AS "ProposerDOB",
       ip.logindt AS "LoginDate",
       ip.insuredgender AS "InsuredGender",
       ip.proposergender AS "ProposerGender",
       ip.maturityageinmonths AS "MaturityAgeInMonths",
       ip.modalbasepremium AS "ModalBasePremium",
       ip.modalbaseriderpremium AS "ModalBaseRiderPremium",
       ip.prod_code AS "ProductCode"
FROM comss.comm_job_exe_dtls cjed
LEFT JOIN hms.agent a
       ON a.agent_id = cjed.agent_id
LEFT JOIN hmsmaster.channel_master cm
       ON cm.channel_id = a.channel
      AND cm.orgid = cjed.orgid
LEFT JOIN ledger_txn lt
       ON lt.orgid = cjed.orgid
      AND lt.agent_id = cjed.agent_id
      AND lt.job_exe_hist_id = cjed.job_exe_hist_id
LEFT JOIN ledger_last ll
       ON ll.orgid = cjed.orgid
      AND ll.agent_id = cjed.agent_id
      AND ll.job_exe_hist_id = cjed.job_exe_hist_id
LEFT JOIN fy_last fl
       ON fl.orgid = cjed.orgid
      AND fl.agent_id = cjed.agent_id
LEFT JOIN insu_core.premium_collected pc
       ON pc.premiucollid = cjed.premiucollid
      AND pc.orgid = cjed.orgid
LEFT JOIN insu_core.ins_policy ip
       ON ip.policyref = pc.policyref
      AND ip.orgid = cjed.orgid
WHERE cjed.orgid = @p_orgid
  AND cjed.job_exe_hist_id = @p_job_exe_hist_id
ORDER BY cjed.comm_job_exe_dtls_id;