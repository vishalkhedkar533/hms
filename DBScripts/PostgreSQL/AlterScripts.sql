ADD COLUMN "FailedLoginAttempts" INTEGER NOT NULL DEFAULT 0,
ADD COLUMN "LockoutEndTime" TIMESTAMP WITH TIME ZONE;
-- 1. Add IS_ACTIVE
ALTER TABLE hms."agent_movement_history"
ADD COLUMN "IS_ACTIVE" BOOLEAN NOT NULL DEFAULT TRUE;

-- 2. Add APPROVED_BY
ALTER TABLE hms."agent_movement_history"
ADD COLUMN "APPROVED_BY" VARCHAR(100);

-- 3. Add APPROVED_DATE
ALTER TABLE hms."agent_movement_history"
ADD COLUMN "APPROVED_DATE" TIMESTAMP;

-- 4. Add REJECTED_BY
ALTER TABLE hms."agent_movement_history"
ADD COLUMN "REJECTED_BY" VARCHAR(100);

-- 5. Add REJECTED_DATE
ALTER TABLE hms."agent_movement_history"
ADD COLUMN "REJECTED_DATE" TIMESTAMP;

-- 6. Add STATUS
ALTER TABLE hms."agent_movement_history"
ADD COLUMN "STATUS" VARCHAR(20) NOT NULL DEFAULT 'Pending';

ALTER TABLE hms."agent" ADD COLUMN "IS_ACTIVE" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE hms.AGENT DROP CONSTRAINT fk_supervisor;
ALTER TABLE hms.AGENT DROP COLUMN SUPERVISOR_CODE;
ALTER TABLE hms.AGENT ADD COLUMN SUPERVISOR_ID INTEGER;

ALTER TABLE hms.AGENT ADD CONSTRAINT fk_supervisor
        FOREIGN KEY (SUPERVISOR_ID)
        REFERENCES hms.AGENT (AGENT_ID)
        ON DELETE SET NULL;

alter table hms.agent drop column "IS_ACTIVE";
alter table hms.agent add column "is_active"  bool DEFAULT true NOT NULL;
ALTER TABLE hms.agent ALTER COLUMN agent_id DROP DEFAULT;

-- Attach identity (auto-increment) to existing column
ALTER TABLE hms.agent ALTER COLUMN agent_id ADD GENERATED ALWAYS AS IDENTITY;

ALTER TABLE hms.agent
ADD COLUMN Email VARCHAR(50),
ADD COLUMN MobileNo VARCHAR(20);

ALTER TABLE hms.hierarchy_node 
ALTER COLUMN node_id 
ADD GENERATED ALWAYS AS IDENTITY;


ALTER TABLE hms.agent_hierarchy 
add CONSTRAINT fk_supervisor
        FOREIGN KEY (SUPERVISOR_CODE)
        REFERENCES hms.AGENT (AGENT_ID)
        ON DELETE cascade;

CREATE EXTENSION IF NOT EXISTS ltree;

ALTER TABLE hms.agent_hierarchy
ADD COLUMN hierarchy_path LTREE;

SELECT * FROM pg_available_extensions WHERE name = 'ltree';

ALTER TABLE hms."agent"
ADD COLUMN candidatype VARCHAR(100),
ADD COLUMN applicationdocketno VARCHAR(100),
ADD COLUMN title VARCHAR(20),
ADD COLUMN father_husband_nm VARCHAR(200),
ADD COLUMN channel_name VARCHAR(100),
ADD COLUMN sub_channel VARCHAR(100),
ADD COLUMN employeecode VARCHAR(50),
ADD COLUMN startdate TIMESTAMP,
ADD COLUMN panaadharlinkflag BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN sec206abflag BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN "packageid" VARCHAR(50),
ADD COLUMN "commissionclass" VARCHAR(50),
ADD COLUMN "taxstatus" VARCHAR(50),
ADD COLUMN "stateeid" VARCHAR(50),
ADD COLUMN "occupationcode" INT,
ADD COLUMN "occupation" VARCHAR(150),
ADD COLUMN "urn" VARCHAR(50),
ADD COLUMN "additionalcomment" VARCHAR(200),
ADD COLUMN "appointmentdate" TIMESTAMP,
ADD COLUMN "incorporationdate" TIMESTAMP,
ADD COLUMN "cnctpersondesig" VARCHAR(100),
ADD COLUMN "cnctpersonmobileno" VARCHAR(20),
ADD COLUMN "cnctpersonemail" VARCHAR(100),
ADD COLUMN "cnctpersonname" VARCHAR(150),
ADD COLUMN "agenttypecategory" VARCHAR(100),
ADD COLUMN "agentclassification" VARCHAR(100),
ADD COLUMN "cmsagenttype" VARCHAR(100),
ADD COLUMN servicetaxno VARCHAR(50),
ADD COLUMN ulipflag BOOLEAN DEFAULT FALSE,
ADD COLUMN traininggrouptype VARCHAR(100),
ADD COLUMN ifs VARCHAR(100),
ADD COLUMN refreshertrainingcompleted BOOLEAN DEFAULT FALSE,
ADD COLUMN ismigrated BOOLEAN DEFAULT FALSE,
ADD COLUMN mainpartnerclientcode VARCHAR(50),
ADD COLUMN agentmaincodevweid VARCHAR(50),
ADD COLUMN registrationdate TIMESTAMP,
ADD COLUMN vertical VARCHAR(100),
ADD COLUMN branchcode VARCHAR(50),
ADD COLUMN branchname VARCHAR(100),
ADD COLUMN ic36trngcompletiondate TIMESTAMP,
ADD COLUMN strngcompletiondate TIMESTAMP,
ADD COLUMN confirmationdate TIMESTAMP,
ADD COLUMN fgrockstartrainingdate TIMESTAMP,
ADD COLUMN incrementdate TIMESTAMP,
ADD COLUMN lastpromotiondate TIMESTAMP,
ADD COLUMN hrdoj TIMESTAMP,
ADD COLUMN fgvaluetrngdate TIMESTAMP,
ADD COLUMN hsecpolicytrngdate TIMESTAMP,
ADD COLUMN itsecpolicytrngdate TIMESTAMP,
ADD COLUMN npstrngcompletiondate TIMESTAMP,
ADD COLUMN whistleblowertrngdate TIMESTAMP,
ADD COLUMN govpolicytrngdate TIMESTAMP,
ADD COLUMN inductiontrngdate TIMESTAMP,
ADD COLUMN lastworkingdate TIMESTAMP,
ADD COLUMN licenseno VARCHAR(50),
ADD COLUMN licensetype VARCHAR(50),
ADD COLUMN licenseissuedate TIMESTAMP,
ADD COLUMN licenseexpirydate TIMESTAMP,
ADD COLUMN licensestatus VARCHAR(50);


ALTER TABLE hms."agent"
ADD COLUMN "ActivePermAddress" INT NULL,
ADD COLUMN "ActiveMailAddress" INT NULL,
ADD CONSTRAINT fk_agent_perm_address
    FOREIGN KEY ("ActivePermAddress") REFERENCES hms."Address"("AddressID"),
ADD CONSTRAINT fk_agent_mail_address
    FOREIGN KEY ("ActiveMailAddress") REFERENCES hms."Address"("AddressID");


alter table hms.tempagentdto 
add column "Comments" varchar,
add column "Reason" varchar

ALTER TABLE hms."agent"
ADD COLUMN "orgid" INT null

alter table hms.tempagentdto 
add column "orgid" INT null

alter table hms.fileprocessingtasks  
add column "orgid" INT null

alter table hms."user" ADD COLUMN OrgId INTEGER;

alter table hms."user" 
add CONSTRAINT fk_User_OrgId
FOREIGN KEY (OrgId) REFERENCES app_subscription.Organisation(OrgId)

alter table hms.agent_hierarchy
add column "orgid" INT null

alter table hms.tempagentdto add Supervisor_Code varchar(50);
alter table hms.tempagentdto drop column Supervisor_Id;

ALTER TABLE hms.BRANCH_MASTER DROP CONSTRAINT fk_branch_head_agent;
ALTER TABLE hms.agent DROP CONSTRAINT agent_agent_code_key;
ALTER TABLE hms.agent ADD CONSTRAINT agent_agent_code_key UNIQUE (agent_code,orgid);
ALTER TABLE hms.BRANCH_MASTER add column "orgid" INT null;
ALTER TABLE hms.BRANCH_MASTER  add CONSTRAINT fk_BrMst_OrgId FOREIGN KEY (OrgId) REFERENCES app_subscription.organisation(OrgId)
ALTER TABLE hms.BRANCH_MASTER add CONSTRAINT fk_branch_head_agent FOREIGN KEY (HEAD_AGENT_ID,OrgId) REFERENCES hms.AGENT (AGENT_CODE,OrgId);

alter table hms.agent drop column agent_type_code;
alter table hms.agent drop column gender;
alter table hms.agent drop column marital_status_code;
alter table hms.agent drop column title;
alter table hms.agent drop column occupationcode;
alter table hms.agent drop column occupation;
alter table hms.agent drop column agent_sub_type_code;
alter table hms.agent drop column designation_code;
alter table hms.agent drop column channel_name;
alter table hms.agent drop column sub_channel;
alter table hms.agent drop column channel_code;
alter table hms.agent drop column sub_channel_code;
alter table hms.agent drop column location_code;

alter table hms.agent add column agent_type_code int4;
alter table hms.agent add column bankacctype int4;
alter table hms.agent add column gender int4;
alter table hms.agent add column title int4;
alter table hms.agent add column channel int4;
alter table hms.agent add column subchannel int4;
alter table hms.agent add column occupation int4;
alter table hms.agent add column agent_type_cat int4;
alter table hms.agent add column agent_class int4;
alter table hms.agent add column martial_status int4;
alter table hms.agent add column education int4;
alter table hms.agent add column state int4;
alter table hms.agent add column country int4;
alter table hms.agent add column agent_sub_type_code int4;
alter table hms.agent add column designation_code int4;
alter table hms.agent add column location_code int4;

alter table hms.agent drop column candidatetype;
alter table hms.agent add column candidatetype int4;
alter table hms.agent add column agenttype int4;
alter table hms.agent drop column CommissionClass;
alter table hms.agent add column commissionclass int4;

alter table hms.individual_commission  
add column "orgId" INT null

ALTER TABLE comss.entity_commission
ADD COLUMN entityCommId serial;

ALTER TABLE comss.entity_commission
ADD CONSTRAINT pk_entity_commission
PRIMARY KEY (entityCommId);

ALTER TABLE comss.commission_config 
ALTER COLUMN orgId SET NOT NULL;

ALTER TABLE comss.commission_config 
ADD COLUMN created_by VARCHAR(255)

CREATE UNIQUE INDEX idx_unique_org_commission 
ON comss.commission_config (orgId, commission_name);


alter table comss.commission_config add column last_run_dt date;
alter table comss.commission_config add column next_run_dt date;

alter table scheduler.job_config add column TargetType VARCHAR(255);
alter table scheduler.job_config add column TargetMethod VARCHAR(100);
alter table scheduler.job_config add column Args VARCHAR(500);
alter table scheduler.job_config add column "orgId" INT null;

alter table scheduler.job_config add CONSTRAINT fk_JonCfg_OrgId
FOREIGN KEY ("orgId") REFERENCES app_subscription.Organisation(OrgId);
ALTER TABLE comss.commission_config 
ADD COLUMN conditions VARCHAR(500);

CREATE UNIQUE INDEX ux_job_config_jobname_orgid
ON scheduler.job_config (job_name, "orgId");

ALTER TABLE comss.commission_config
ADD COLUMN job_config_id int

ALTER TABLE comss.commission_config
ADD CONSTRAINT fk_commission_config_job_config
FOREIGN KEY (job_config_id)
REFERENCES scheduler.job_config(job_config_id)

ALTER TABLE comss.commission_config
DROP COLUMN trigger_cycle;

alter table insu_core.policy
add column isStaffPolicy bool;

alter table insu_core.policy
add column PolicySourceCode int;

alter table insu_core.policy
add column insuredPAN varchar(10);

alter table insu_core.policy
add column proposerPAN varchar(10);

alter table insu_core.policy
add column insuredDOB date;

alter table insu_core.policy
add column proposerDOB date;

alter table insu_core.policy
add column loginDt date;

alter table insu_core.policy
add column insuredGender int;

alter table insu_core.policy
add column proposerGender int;

alter table insu_core.policy
add column maturityAgeInMonths int;

alter table insu_core.policy
add column modalBasePremium decimal;

alter table insu_core.policy
add column modalBaseRiderPremium decimal;

alter table hms.agent add column ActivePermAddress int4 null;
alter table hms.agent add column ActiveMailAddress int4 null;
update hms.agent set ActivePermAddress = "ActivePermAddress";
update hms.agent set ActiveMailAddress = "ActiveMailAddress";
alter table hms.agent drop column "ActivePermAddress";
alter table hms.agent drop column "ActiveMailAddress";

ALTER TABLE comss.commission_config 
ADD COLUMN filter_condition VARCHAR(500);

ALTER TABLE comss.commission_config 
ADD COLUMN comments VARCHAR(2000) NULL;

ALTER TABLE scheduler.job_config
ADD COLUMN comments VARCHAR(2000) NULL;
alter table insu_core.ins_policy add column prod_code varchar(10)

ALTER TABLE scheduler.job_config
DROP COLUMN comments;

ALTER TABLE comss.commission_config
DROP COLUMN filter_condition;

ALTER TABLE comss.commission_config
DROP COLUMN comments;
