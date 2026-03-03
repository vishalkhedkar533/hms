WITH master_map AS (
    SELECT
        t.agentcode,
        t.orgid,
        cm.channel_id AS channel_id,
        cm.channel_code AS channel_code,
        scm.sub_channel_id AS sub_channel_id,
        dm.designation_code AS designation_code,
        dm.designation_id AS designation_id,
        lm.location_master_id AS location_id,
        bm.branch_id AS branch_id,
        MAX(CASE WHEN k.entrycategory = 'BANK_ACC_TYP' THEN k.entryidentity END) AS bankacctype,
        MAX(CASE WHEN k.entrycategory = 'AGENT_TYPE_CAT' THEN k.entryidentity END) AS agent_type_cat,
        MAX(CASE WHEN k.entrycategory = 'AGENT_CLASS' THEN k.entryidentity END) AS agent_class,
        MAX(CASE WHEN k.entrycategory = 'MARITAL_STATUS' THEN k.entryidentity END) AS marital_status,
        MAX(CASE WHEN k.entrycategory = 'EDUCATION_CODE' THEN k.entryidentity END) AS education,
        MAX(CASE WHEN k.entrycategory = 'STATE_NAME' THEN k.entryidentity END) AS state,
        MAX(CASE WHEN k.entrycategory = 'COUNTRY' THEN k.entryidentity END) AS country,
        MAX(CASE WHEN k.entrycategory = 'GENDER' THEN k.entryidentity END) AS gender,
        MAX(CASE WHEN k.entrycategory = 'TITLE' THEN k.entryidentity END) AS title,
        MAX(CASE WHEN k.entrycategory = 'OCCUPATION' THEN k.entryidentity END) AS occupation,
        MAX(CASE WHEN k.entrycategory = 'AGENT_SUB_TYPE_CODE' THEN k.entryidentity END) AS agent_sub_type_code,
        MAX(CASE WHEN k.entrycategory = 'AGENT_TYPE_CODE' THEN k.entryidentity END) AS agent_type_code,
        MAX(CASE WHEN k.entrycategory = 'CANDIDATE_TYP' THEN k.entryidentity END) AS candidatetype,
        MAX(CASE WHEN k.entrycategory = 'AGNT_TYP' THEN k.entryidentity END) AS agenttype,
        MAX(CASE WHEN k.entrycategory = 'COMMISSION_CLASS' THEN k.entryidentity END) AS commissionclass,
        MAX(CASE WHEN k.entrycategory = 'LICENSE_TYPE' THEN k.entryidentity END) AS licensetype_id,
        MAX(CASE WHEN k.entrycategory = 'LICENSE_STATUS' THEN k.entryidentity END) AS licensestatus_id,
        MAX(CASE WHEN k.entrycategory = 'VERTICAL' THEN k.entryidentity END) AS vertical_id,
        MAX(CASE WHEN k.entrycategory = 'TRAINING_GROUP' THEN k.entryidentity END) AS traininggrouptype_id,
        MAX(CASE WHEN k.entrycategory = 'STATE_NAME' THEN k.entrydesc END) AS state_desc,
        MAX(CASE WHEN k.entrycategory = 'COUNTRY' THEN k.entrydesc END) AS country_desc
    FROM hms.tempagentdto t
    LEFT JOIN hmsmaster.channel_master cm 
        ON UPPER(TRIM(t.channel_desc)) = UPPER(TRIM(cm.channel_name)) 
        AND t.orgid = cm.orgid
    LEFT JOIN hmsmaster.subchannel_master scm 
        ON UPPER(TRIM(t.sub_channel_desc)) = UPPER(TRIM(scm.subchannel_name)) 
        AND cm.channel_id = scm.channel_id AND t.orgid = scm.orgid
    LEFT JOIN hmsmaster.designation_master dm 
        ON UPPER(TRIM(t.designation_code_desc)) = UPPER(TRIM(dm.designation_name)) 
        AND t.orgid = dm.orgid
    LEFT JOIN hmsmaster.location_master lm 
        ON UPPER(TRIM(t.location_code_desc)) = UPPER(TRIM(lm.location_desc)) 
        AND t.orgid = lm.orgid 
        AND lm.channel_id = cm.channel_id
    LEFT JOIN hmsmaster.branch_master bm 
        ON UPPER(TRIM(t.branch_desc)) = UPPER(TRIM(bm.branch_name)) 
        AND t.orgid = bm.orgid 
        AND lm.location_master_id = bm.location_master_id
    LEFT JOIN hmsmaster.keyvalueentries k 
        ON k.orgid = t.orgid
        AND (
               (k.entrycategory = 'BANK_ACC_TYP' AND k.entrydesc = t.bank_acc_type_desc)
            OR (k.entrycategory = 'AGENT_TYPE_CAT' AND k.entrydesc = t.agent_type_cat_desc)
            OR (k.entrycategory = 'AGENT_CLASS' AND k.entrydesc = t.agent_class_desc)
            OR (k.entrycategory = 'MARITAL_STATUS' AND k.entrydesc = t.marital_status_desc)
            OR (k.entrycategory = 'EDUCATION_CODE' AND k.entrydesc = t.education_desc)
            OR (k.entrycategory = 'STATE_NAME' AND k.entrydesc = t.state_desc)
            OR (k.entrycategory = 'COUNTRY' AND k.entrydesc = t.country_desc)
            OR (k.entrycategory = 'GENDER' AND k.entrydesc = t.gender_desc)
            OR (k.entrycategory = 'TITLE' AND k.entrydesc = t.title_desc)
            OR (k.entrycategory = 'OCCUPATION' AND k.entrydesc = t.occupation_desc)
            OR (k.entrycategory = 'AGENT_SUB_TYPE_CODE' AND k.entrydesc = t.agent_sub_type_code_desc)
            OR (k.entrycategory = 'AGENT_TYPE_CODE' AND k.entrydesc = t.agent_type_code_desc)
            OR (k.entrycategory = 'CANDIDATE_TYP' AND t.candidate_type_desc = k.entrydesc)
            OR (k.entrycategory = 'AGNT_TYP' AND t.agent_type_desc = k.entrydesc)
            OR (k.entrycategory = 'COMMISSION_CLASS' AND k.entrydesc = t.commission_class_desc)
            OR (k.entrycategory = 'LICENSE_TYPE' AND k.entrydesc = t.licensetype)
            OR (k.entrycategory = 'LICENSE_STATUS' AND k.entrydesc = t.licensestatus)
            OR (k.entrycategory = 'VERTICAL' AND k.entrydesc = t.vertical)
            OR (k.entrycategory = 'TRAINING_GROUP' AND k.entrydesc = t.traininggrouptype)
        )
    WHERE t.comments = 'Processed'
    GROUP BY 
        t.agentcode, t.orgid, cm.channel_id, cm.channel_code, scm.sub_channel_id, 
        dm.designation_code, dm.designation_id, lm.location_master_id, bm.branch_id
),
ins AS (
    INSERT INTO hms.agent (
        agent_code, agent_name, business_name, first_name, middle_name, last_name, prefix,
        suffix, dob, nationality, preferred_language, agent_level,
        staff_code, contracted_date, agent_status_code, status_date,
        is_licensed, pan_number, aadhaar_number, irda_license_number, gst_number,
        created_by, created_date, modified_by, modified_date, rowversion,
        supervisor_id, is_active, applicationdocketno, father_husband_nm,
        employeecode, startdate, panaadharlinkflag, sec206abflag,
        taxstatus, urn, additionalcomment, appointmentdate, incorporationdate,
        cnctpersondesig, cnctpersonmobileno, cnctpersonemail, cnctpersonname,
        cmsagenttype, packageid, servicetaxno, ulipflag, traininggrouptype, ifs,
        refreshertrainingcompleted, ismigrated, mainpartnerclientcode,
        agentmaincodevweid, registrationdate, vertical, branchcode, branchname,
        ic36trngcompletiondate, strngcompletiondate, confirmationdate,
        fgrockstartrainingdate, incrementdate, lastpromotiondate, hrdoj,
        fgvaluetrngdate, hsecpolicytrngdate, itsecpolicytrngdate,
        npstrngcompletiondate, whistleblowertrngdate, govpolicytrngdate,
        inductiontrngdate, lastworkingdate, licenseno, licensetype, licenseissuedate, licenseexpirydate,
        licensestatus, orgid, bankacctype, channel, subchannel,
        agent_type_cat, agent_class, martial_status, education,
        state, country, gender, title, occupation,
        agent_sub_type_code, designation_code, agent_type_code,
        location_code, candidatetype, agenttype, commissionclass, branch
    )
    SELECT DISTINCT ON (t.agentcode)
        t.agentcode, t.agentname, t.businessname, t.firstname, t.middlename, t.lastname, t.prefix,
        t.suffix, NULLIF(TRIM(t.dob), '')::date, t.nationality, t.preferredlanguage,
        CASE TRIM(t.agentlevel) WHEN 'Level1' THEN 1 WHEN 'Level2' THEN 2 WHEN 'Level3' THEN 3 ELSE NULL END,
        t.staffcode, NULLIF(TRIM(t.contracteddate), '')::date, t.agentstatuscode, NULLIF(TRIM(t.statusdate), '')::date,
        CASE WHEN t.islicensed IN ('Y','y','1','TRUE','true','T') THEN TRUE ELSE FALSE END,
        t.maskedpannumber, t.aadhaar_number, t.irdalicensenumber, t.gstnumber,
        t.createdby, NULLIF(TRIM(t.createddate), '')::date, t.modifiedby, NULLIF(TRIM(t.modifieddate), '')::date,
        NULLIF(TRIM(t.rowversion), '')::integer, 
        sup.agent_id::integer,
        CASE WHEN t.isactive IN ('Y','y','1','TRUE','true','T') THEN TRUE ELSE FALSE END,
        t.applicationdocketno, t.father_husband_nm, t.employeecode, NULLIF(TRIM(t.startdate), '')::date,
        CASE WHEN t.panaadharlinkflag IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
        CASE WHEN t.sec206abflag IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
        t.taxstatus, t.urn, t.additionalcomment, NULLIF(TRIM(t.appointmentdate), '')::date, NULLIF(TRIM(t.incorporationdate), '')::date,
        t.cnctpersondesig, t.cnctpersonmobileno, t.cnctpersonemail, t.cnctpersonname,
        t.cmsagenttype, t.packageid, t.servicetaxno,
        CASE WHEN t.ulipflag IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
        m.traininggrouptype_id, t.ifs,
        CASE WHEN t.refreshertrainingcompleted IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
        CASE WHEN t.ismigrated IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
        t.mainpartnerclientcode, t.agentmaincodevweid, NULLIF(TRIM(t.registrationdate), '')::date,
        m.vertical_id, t.branchcode, t.branchname, NULLIF(TRIM(t.ic36trngcompletiondate), '')::date,
        NULLIF(TRIM(t.strngcompletiondate), '')::date, NULLIF(TRIM(t.confirmationdate), '')::date,
        NULLIF(TRIM(t.fgrockstartrainingdate), '')::date, NULLIF(TRIM(t.incrementdate), '')::date,
        NULLIF(TRIM(t.lastpromotiondate), '')::date, NULLIF(TRIM(t.hrdoj), '')::date,
        NULLIF(TRIM(t.fgvaluetrngdate), '')::date, NULLIF(TRIM(t.hsecpolicytrngdate), '')::date,
        NULLIF(TRIM(t.itsecpolicytrngdate), '')::date, NULLIF(TRIM(t.npstrngcompletiondate), '')::date,
        NULLIF(TRIM(t.whistleblowertrngdate), '')::date, NULLIF(TRIM(t.govpolicytrngdate), '')::date,
        NULLIF(TRIM(t.inductiontrngdate), '')::date, NULLIF(TRIM(t.lastworkingdate), '')::date,
        t.licenseno, m.licensetype_id, NULLIF(TRIM(t.licenseissuedate), '')::date, NULLIF(TRIM(t.licenseexpirydate), '')::date,
        m.licensestatus_id, t.orgid, m.bankacctype, m.channel_id, m.sub_channel_id,
        m.agent_type_cat, m.agent_class, m.marital_status, m.education, m.state, m.country, m.gender,
        m.title, m.occupation, m.agent_sub_type_code, m.designation_id, m.agent_type_code,
        m.location_id, m.candidatetype, m.agenttype, m.commissionclass, m.branch_id
    FROM hms.tempagentdto t
    LEFT JOIN LATERAL (
        SELECT agent_id FROM hms.agent 
        WHERE agent_code = t.Supervisor_Code 
          AND t.Supervisor_Code IS NOT NULL 
          AND t.Supervisor_Code <> ''
          AND orgid = t.orgid
        LIMIT 1
    ) sup ON TRUE
    LEFT JOIN master_map m ON m.agentcode = t.agentcode AND m.orgid = t.orgid
    WHERE t.comments = 'Processed'
    ORDER BY t.agentcode, t.agentid ASC
    ON CONFLICT ON CONSTRAINT agent_agent_code_key DO NOTHING
    RETURNING 
        agent_id, 
        agent_code, 
        supervisor_id, 
        created_by, 
        orgid,
        (SELECT m2.designation_code FROM master_map m2 WHERE m2.agentcode = hms.agent.agent_code AND m2.orgid = hms.agent.orgid LIMIT 1) AS ret_designation_code,
        (SELECT m3.channel_code FROM master_map m3 WHERE m3.agentcode = hms.agent.agent_code AND m3.orgid = hms.agent.orgid LIMIT 1) AS ret_channel_code
),
hierarchy_ins AS (
    INSERT INTO hms.agent_hierarchy (agent_id, channel_code, effective_from_date, designation_code, created_by, created_date, rowversion, hierarchy_path, orgid)
    SELECT
        i.agent_id, 
        i.ret_channel_code, 
        CURRENT_DATE, 
        i.ret_designation_code, 
        COALESCE(i.created_by, ''), 
        NOW(), 
        1,
        CASE 
            WHEN i.supervisor_id IS NULL THEN i.agent_id::text::ltree 
            ELSE (
                SELECT (h.hierarchy_path::text || '.' || i.agent_id::text)::ltree 
                FROM hms.agent_hierarchy h 
                WHERE h.agent_id = i.supervisor_id 
                  AND (h.orgid = i.orgid OR h.orgid IS NULL)
                LIMIT 1
            ) 
        END,
        i.orgid
    FROM ins i
),
nominee_ins AS (
    INSERT INTO hms."Nominee" (
        "RefKey", "RefType", "NomineeName", "Relationship", "PercentageShare", "NomineeAge", "IsActive", "orgId"
    )
    SELECT DISTINCT ON (i.agent_id)
        i.agent_id, 1, t.nomineename, t.relationship, NULLIF(t.percentageshare, '')::numeric(5,2), 
        NULLIF(t.nomineeage, '')::bigint, TRUE, t.orgid
    FROM ins i
    JOIN hms.tempagentdto t ON t.agentcode = i.agent_code AND t.orgid = i.orgid
    LEFT JOIN hms."Nominee" n ON n."RefKey" = i.agent_id AND n."RefType" = 1 AND n."orgId" = i.orgid
    WHERE t.nomineename IS NOT NULL AND TRIM(t.nomineename) <> '' AND n."NomineeID" IS NULL
    ORDER BY i.agent_id, t.agentid ASC
),
bank_ins AS (
    INSERT INTO hms.bankaccount (
        refkey, reftype, accountholdername, accountnumber, ifsc, micr, bankname, branchname, 
        accounttype, activesince, factoringhouse, preferredpaymentmode, orgid
    )
    SELECT DISTINCT ON (i.agent_id) 
        i.agent_id, 1, t.accountholdername, t.accountnumber, t.ifsc, t.micr, t.bankname, t.Accbranchname, 
        COALESCE(NULLIF(t.accounttype, '')::int, 1), 
        COALESCE(NULLIF(TRIM(t.activesince), '')::timestamp, CURRENT_TIMESTAMP), 
        t.factoringhouse, 
        COALESCE(NULLIF(t.preferredpaymentmode, '')::int, 1), 
        t.orgid
    FROM ins i
    JOIN hms.tempagentdto t ON t.agentcode = i.agent_code AND t.orgid = i.orgid
    LEFT JOIN hms.bankaccount b ON b.refkey = i.agent_id AND b.reftype = 1 AND b.orgid = i.orgid
    WHERE t.accountnumber IS NOT NULL AND TRIM(t.accountnumber) <> '' AND t.ifsc IS NOT NULL AND TRIM(t.ifsc) <> '' AND b.id IS NULL
    ORDER BY i.agent_id, t.agentid ASC
    ON CONFLICT (refkey, reftype) DO NOTHING
),
personalinfo_ins AS (
    INSERT INTO hms."PersonalInfo" (
        "RefKey", "RefType", "DateOfBirth", "PanNumber", "Email", "MobileNo", "WorkContactNo", 
        "ResidenceContactNo", "BloodGroup", "BirthPlace", "MartialStatus", "EducationCode", 
        "EducationLevel", "WorkProfile", "AnnualIncome", "WorkExpMonths", "orgId"
    )
    SELECT DISTINCT ON (i.agent_id)
        i.agent_id, 1, NULLIF(t.dateofbirth,'')::date, t.pannumber, t.email, t.mobileno, 
        t.workcontactno, t.residencecontactno, t.bloodgroup, t.birthplace, m.marital_status, 
        m.education, t.educationlevel, t.workprofile, NULLIF(t.annualincome,'')::numeric(18,2), 
        NULLIF(t.workexpmonths,'')::int, t.orgid
    FROM ins i
    JOIN hms.tempagentdto t ON t.agentcode = i.agent_code AND t.orgid = i.orgid
    LEFT JOIN hms."PersonalInfo" p ON p."RefKey" = i.agent_id AND p."RefType" = 1 AND p."orgId" = i.orgid
    LEFT JOIN LATERAL (
        SELECT 
            MAX(CASE WHEN entrycategory = 'MARITAL_STATUS' THEN entryidentity END) AS marital_status, 
            MAX(CASE WHEN entrycategory = 'EDUCATION_CODE' THEN entryidentity END) AS education 
        FROM hmsmaster.keyvalueentries k 
        WHERE k.orgid = t.orgid 
          AND ((k.entrycategory = 'MARITAL_STATUS' AND k.entrydesc = t.marital_status_desc) 
                OR (k.entrycategory = 'EDUCATION_CODE' AND k.entrydesc = t.pinfo_education_desc))
    ) m ON TRUE
    WHERE p."PersonalInfoId" IS NULL AND (t.email IS NOT NULL OR t.mobileno IS NOT NULL OR t.dateofbirth IS NOT NULL)
    ORDER BY i.agent_id, t.agentid ASC
    ON CONFLICT ("RefKey", "RefType") DO NOTHING
),
address_ins AS (
    INSERT INTO hms."Address" (
        "RefKey", "RefType", "AddressType", "AddressLine1", "AddressLine2", "AddressLine3", 
        "City", "State", "Country", "PIN", "Landmark", "orgId"
    )
    SELECT DISTINCT ON (i.agent_id) 
        i.agent_id, 1, 3, t.addressline1, t.addressline2, t.addressline3, t.city, m.state_desc, 
        m.country_desc, t.pin, t.landmark, t.orgid
    FROM ins i
    JOIN hms.tempagentdto t ON t.agentcode = i.agent_code AND t.orgid = i.orgid
    LEFT JOIN hms."Address" a ON a."RefKey" = i.agent_id AND a."RefType" = 1 AND a."AddressType" = 3
    LEFT JOIN LATERAL (
        SELECT 
            MAX(CASE WHEN entrycategory = 'STATE_NAME' THEN entrydesc END) AS state_desc, 
            MAX(CASE WHEN entrycategory = 'COUNTRY'    THEN entrydesc END) AS country_desc 
        FROM hmsmaster.keyvalueentries k 
        WHERE k.orgid = t.orgid 
          AND ((k.entrycategory = 'STATE_NAME' AND k.entrydesc = t.state_desc) 
                OR (k.entrycategory = 'COUNTRY'     AND k.entrydesc = t.country_desc))
    ) m ON TRUE
    WHERE a."AddressID" IS NULL AND (t.addressline1 IS NOT NULL OR t.city IS NOT NULL OR t.pin IS NOT NULL)
    ORDER BY i.agent_id, t.agentid ASC
)
SELECT 1;