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

alter table insu_core.ins_policy add column prod_code varchar(10);

alter table comss.comm_rate add column pol_yr_from int4 null;
alter table comss.comm_rate add column pol_yr_to int4 null;

alter table insu_core.premium_collected add column prem_coll_yr int4 null;
alter table insu_core.premium_collected add column prem_coll_qtr int4 null;
alter table insu_core.premium_collected add column prem_coll_fin_yr varchar(10) null;

alter table comss.comm_rate alter column pol_yr_from set not null;
alter table comss.comm_rate alter column pol_yr_to set not null;

alter table insu_core.premium_collected alter column prem_coll_yr set not null;
alter table insu_core.premium_collected alter column prem_coll_qtr set not null;
alter table insu_core.premium_collected alter column prem_coll_fin_yr set not null;

alter table comss.commission_config alter conditions TYPE varchar(10000);
alter table comss.commission_config add formula varchar(10000);
alter table comss.commission_config drop column conditions ;

alter table insu_core.tmp_ins_policy add column prod_code varchar(10)
alter table comss.commission_config drop column conditions ;

ALTER TABLE comss.commission_config DROP COLUMN run_from;
ALTER TABLE comss.commission_config DROP COLUMN run_to;
ALTER TABLE comss.commission_config DROP COLUMN next_run_dt;
ALTER TABLE comss.commission_config DROP COLUMN last_run_dt;
ALTER TABLE comss.commission_config DROP COLUMN commission_name;


alter TABLE scheduler.job_exe_hist add FireInstanceId bigint;
alter TABLE scheduler.job_exe_hist add TriggerObject varchar(4000);
alter TABLE scheduler.job_exe_hist add JobDetailObject varchar(4000);
alter TABLE scheduler.job_exe_hist add FireTimeUtc TIMESTAMP WITH TIME ZONE;
alter TABLE scheduler.job_exe_hist add LogLevel varchar(10);
create index idx_FireInstanceId on scheduler.job_exe_hist(FireInstanceId);

ALTER TABLE comss.comm_job_exe_dtls add status varchar (20) not null default 'PENDING' /*PENDING/APPROVED/HOLD/REJECTED*/;

ALTER TABLE hms.agent DROP COLUMN vertical;
ALTER TABLE hms.agent DROP COLUMN licensetype;
ALTER TABLE hms.agent DROP COLUMN licensestatus;
ALTER TABLE hms.agent DROP COLUMN traininggrouptype;


ALTER TABLE hms.agent ADD COLUMN vertical int4;
ALTER TABLE hms.agent ADD COLUMN licensetype int4;
ALTER TABLE hms.agent ADD COLUMN licensestatus int4;
ALTER TABLE hms.agent ADD COLUMN traininggrouptype int4;

ALTER TABLE hms.agent_audit_trail 
ALTER COLUMN audit_id ADD GENERATED BY DEFAULT AS IDENTITY;


ALTER TABLE hms.CHANNEL_MASTER ADD COLUMN "orgid" INT null;
ALTER TABLE hms.CHANNEL_MASTER add CONSTRAINT fk_chann_mst_org
FOREIGN KEY (orgId) REFERENCES app_subscription.organisation(orgId);

ALTER TABLE hms.agent_type_master DROP CONSTRAINT fk_agent_type_channel;
ALTER TABLE hms.branch_master DROP CONSTRAINT fk_branch_channel;
ALTER TABLE hms.onholdpayouts DROP CONSTRAINT fk_channel;
ALTER TABLE hms.geo_hierarchy DROP CONSTRAINT fk_channel_code;
ALTER TABLE hms.designation_master DROP CONSTRAINT fk_channel_code;
ALTER TABLE hms.subchannel_master DROP CONSTRAINT fk_channel_code;

ALTER TABLE hms.CHANNEL_MASTER DROP CONSTRAINT channel_master_pkey;

ALTER TABLE hms.channel_master SET SCHEMA hmsmaster;

ALTER TABLE hms.subchannel_master SET SCHEMA hmsmaster;

CREATE SEQUENCE hmsmaster.channel_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 9223372036854775807
	START 1
	CACHE 1
	NO CYCLE; 

ALTER TABLE hmsmaster.channel_master add column channel_id int8 DEFAULT nextval('hmsmaster.channel_id_seq'::regclass) not null PRIMARY key;

ALTER TABLE hmsmaster.channel_master ADD CONSTRAINT uq_channel_org UNIQUE (channel_code, orgid);

alter table hmsmaster.subchannel_master add column channel_id int8;
alter table hmsmaster.subchannel_master add CONSTRAINT fk_channel_id FOREIGN KEY (channel_id) REFERENCES hmsmaster.channel_master (channel_id);

ALTER TABLE hms.agent_type_master ADD channel_id int8;
ALTER TABLE hms.branch_master ADD channel_id int8;
ALTER TABLE hms.onholdpayouts ADD channel_id int8;
ALTER TABLE hms.geo_hierarchy ADD channel_id int8;
ALTER TABLE hms.designation_master ADD channel_id int8;

ALTER TABLE hms.agent_type_master ADD CONSTRAINT fk_agent_type_channel FOREIGN KEY (channel_id) REFERENCES hmsmaster.channel_master(channel_id);
ALTER TABLE hms.branch_master ADD CONSTRAINT fk_branch_channel FOREIGN KEY (channel_id) REFERENCES hmsmaster.channel_master(channel_id);
ALTER TABLE hms.onholdpayouts ADD CONSTRAINT payput FOREIGN KEY (channel_id) REFERENCES hmsmaster.channel_master(channel_id);
ALTER TABLE hms.geo_hierarchy ADD CONSTRAINT fk_geo_channel FOREIGN KEY (channel_id) REFERENCES hmsmaster.channel_master(channel_id);
ALTER TABLE hms.designation_master ADD CONSTRAINT fk_desig_channel FOREIGN KEY (channel_id) REFERENCES hmsmaster.channel_master(channel_id);


CREATE SEQUENCE hmsmaster.subchannel_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 9223372036854775807
	START 1
	CACHE 1
	NO CYCLE; 

ALTER TABLE hmsmaster.subchannel_master DROP CONSTRAINT subchannel_master_pkey;

ALTER TABLE hmsmaster.subchannel_master add column sub_channel_id int8 
DEFAULT nextval('hmsmaster.subchannel_id_seq'::regclass) not null PRIMARY key;

ALTER TABLE hmsmaster.designation_master  ADD COLUMN "orgid" INT null;
ALTER TABLE hmsmaster.designation_master add CONSTRAINT fk_chann_mst_org
FOREIGN KEY (orgId) REFERENCES app_subscription.organisation(orgId);

alter table hmsmaster.mastertables add columnalias varchar(3000)
alter table comss.comm_job_exe_dtls add ProfTax decimal DEFAULT 0 null;
alter table comss.comm_job_exe_dtls add tds decimal DEFAULT 0 null;
alter table comss.comm_job_exe_dtls add igst decimal DEFAULT 0 null;
alter table comss.comm_job_exe_dtls add cgst decimal DEFAULT 0 null;
alter table comss.comm_job_exe_dtls add sgst decimal DEFAULT 0 null;
alter table comss.comm_job_exe_dtls add ugst decimal DEFAULT 0 null;

ALTER TABLE hms.fileprocessingtasks ADD COLUMN filetype varchar(100) NOT NULL DEFAULT 'unknown';
alter table comss.comm_job_exe_dtls add ugst decimal DEFAULT 0 null;

alter table app_subscription.organisation add column state int4;

ALTER TABLE hmsmaster.financialperiod add column igst decimal default 0;
ALTER TABLE hmsmaster.financialperiod add column sgst decimal default 0;
ALTER TABLE hmsmaster.financialperiod add column cgst decimal default 0;
ALTER TABLE hmsmaster.financialperiod add column ugst decimal default 0;

ALTER TABLE hmsmaster.channel_master 
ADD COLUMN total_entries int8 DEFAULT 0,
ADD COLUMN total_entries_mon int8 DEFAULT 0;
alter table insu_core.premium_collected add column trans_id int4 default 0;

ALTER TABLE hms.fileprocessingtasks 
ADD COLUMN IF NOT EXISTS successdata text;

ALTER TABLE hms.role_master 
ALTER COLUMN role_id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER COLUMN role_id ADD GENERATED BY DEFAULT AS IDENTITY;

alter table hms.agent add column branch int4;

alter table hms.tempagentdto  add column branch_desc varchar(100);

alter table hmsmaster.designation_master add column code_format varchar(5);

ALTER TABLE hms.agent ADD CONSTRAINT fk_agnt_loc FOREIGN KEY (location_code) REFERENCES hmsmaster.location_master(location_master_id);
ALTER TABLE hms.agent ADD CONSTRAINT fk_agnt_br FOREIGN KEY (branch) REFERENCES hmsmaster.branch_master(branch_id);

alter table hms.roles add column orgid int8 default 0;
ALTER TABLE hms.roles add CONSTRAINT fk_role_org FOREIGN KEY (orgid) REFERENCES app_subscription.organisation(orgId);

alter table hms.role_menu_mapping add column orgid int4 default 0;
ALTER TABLE hms.role_menu_mapping add CONSTRAINT fk_role_org FOREIGN KEY (orgid) REFERENCES app_subscription.organisation(orgId);

alter table hms.user_role_mapping add column orgid int4 default 0;	
ALTER TABLE hms.user_role_mapping add CONSTRAINT fk_role_org FOREIGN KEY (orgid) REFERENCES app_subscription.organisation(orgId);

ALTER TABLE hmsmaster.branch_master DROP CONSTRAINT branch_uq;
DROP INDEX hmsmaster.branch_uq;

CREATE UNIQUE INDEX branch_uq ON hmsmaster.branch_master USING btree (orgid, branch_code,location_master_id);

alter table hmsmaster.designation_master add column sub_channel_id int8;
ALTER TABLE hmsmaster.subchannel_master add CONSTRAINT fk_des_subchan 
FOREIGN KEY (sub_channel_id) REFERENCES hmsmaster.subchannel_master(sub_channel_id);

ALTER TABLE hmsmaster.designation_master DROP CONSTRAINT designation_master_uq;
ALTER TABLE hmsmaster.designation_master ADD CONSTRAINT designation_master_uq UNIQUE (designation_code,channel_id,sub_channel_id);

CREATE SEQUENCE hms.rolemapping_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 9223372036854775807
	START 1
	CACHE 1
	NO CYCLE; 

ALTER TABLE hms.role_menu_mapping 
    ALTER COLUMN mapping_id SET DEFAULT nextval('hms.rolemapping_seq'::regclass),
    ALTER COLUMN mapping_id SET NOT NULL;

alter table hms.temp_manager_update  
add column comments varchar,
add column reason varchar

ALTER TABLE hms.agent 
ADD CONSTRAINT uq_agent_code UNIQUE (agent_code, orgid)

ALTER TABLE hms.temp_designation_update 
ADD CONSTRAINT fk_temp_designation_agent 
FOREIGN KEY (agent_code, orgid) 
REFERENCES hms.agent (agent_code, orgid);

ALTER TABLE hms.temp_manager_update 
ADD CONSTRAINT fk_temp_manager_agent 
FOREIGN KEY (agent_code, orgid) 
REFERENCES hms.agent (agent_code, orgid);

ALTER TABLE hms.temp_status_update 
ADD CONSTRAINT fk_temp_status_agent 
FOREIGN KEY (agent_code, orgid) 
REFERENCES hms.agent (agent_code, orgid);

alter table hmsmaster.designation_master ADD CONSTRAINT fk_desig_subchannel
FOREIGN KEY (sub_channel_id) 
REFERENCES hmsmaster.subchannel_master (sub_channel_id);

update hmsmaster.location_master set   created_by = '1', modified_by = '1';
-- 2. Alter the columns with an explicit type cast and add the foreign key
ALTER TABLE hmsmaster.location_master 
    ALTER COLUMN created_by TYPE int4 USING (created_by::int4),
    ALTER COLUMN modified_by TYPE int4 USING (modified_by::int4);

-- 3. Add the Foreign Key constraints
ALTER TABLE hmsmaster.location_master 
    ADD CONSTRAINT fk_location_created_by FOREIGN KEY (created_by) REFERENCES hms."user"(user_id),
    ADD CONSTRAINT fk_location_modified_by FOREIGN KEY (modified_by) REFERENCES hms."user"(user_id);


update hmsmaster.branch_master set   created_by = '1', modified_by = '1';
ALTER TABLE hmsmaster.branch_master
    ALTER COLUMN created_by TYPE int4 USING (created_by::int4),
    ALTER COLUMN modified_by TYPE int4 USING (modified_by::int4);
ALTER TABLE hmsmaster.branch_master 
    ADD CONSTRAINT fk_location_created_by FOREIGN KEY (created_by) REFERENCES hms."user"(user_id),
    ADD CONSTRAINT fk_location_modified_by FOREIGN KEY (modified_by) REFERENCES hms."user"(user_id);

ALTER TABLE hmsmaster.location_master 
    ALTER COLUMN orgid SET NOT NULL;

ALTER TABLE hmsmaster.branch_master ADD hierarchy_path public.ltree NULL;

ALTER TABLE hmsmaster.branch_master 
DROP COLUMN hierarchy_path;

ALTER TABLE hms.inbox 
DROP COLUMN cntrl_id;

ALTER TABLE hms.inbox 
DROP COLUMN role_id;

alter table hms.inbox add cntrl_id int4 references hmsmaster.ui_fields(cntrl_id);
ALTER TABLE hms.inbox ALTER COLUMN cntrl_id SET NOT NULL;

alter table hms.inbox add column allocated_to_role int4 references hms.roles(role_id);

alter table hms.sr_approver add column approver_decision int4;

alter TABLE hms."user" add reporting_mgr int4 null references hms."user"(user_id);

alter table hms.sr_approver drop column	approvalpayload;
alter table hms.sr_approver drop column	approvalapiresponse;
alter table hms.sr_approver drop column approvalendpoint;

alter table hms.inbox add column approvalendpoint varchar(2000);
alter table hms.inbox add column approvalpayload text null;
alter table hms.inbox add column approvalapiresponse text null;

alter table hms.inbox add column object_name varchar(100) null;
alter table hms.inbox drop column object_new_value text null;

alter table hmsmaster.ui_fields drop column object_name  varchar(100) null;
alter table hmsmaster.ui_fields drop column object_field varchar(100) null;

update hmsmaster.ui_fields set object_name = 'hms.agent';

-- 2. Alter the columns with an explicit type cast and add the foreign key
ALTER TABLE hms."user"
    ALTER COLUMN created_by TYPE int4 USING (created_by::int4),
    ALTER COLUMN modified_by TYPE int4 USING (modified_by::int4);

-- 3. Add the Foreign Key constraints
ALTER TABLE hms."user"
    ADD CONSTRAINT fk_location_created_by FOREIGN KEY (created_by) REFERENCES hms."user"(user_id),
    ADD CONSTRAINT fk_location_modified_by FOREIGN KEY (modified_by) REFERENCES hms."user"(user_id);

alter table hmsmaster.channel_branch_heirarchy add branch_id int8 references hmsmaster.branch_master(branch_id);

CREATE UNIQUE INDEX idx_unique_ch_br_hier ON  hmsmaster.channel_branch_heirarchy(orgId,channel_id,sub_channel_id,branch_id );


-- 2. Alter the columns with an explicit type cast and add the foreign key
ALTER TABLE hmsmaster.channel_branch_heirarchy
    ALTER COLUMN created_by TYPE int4 USING (created_by::int4),
    ALTER COLUMN modified_by TYPE int4 USING (modified_by::int4);

-- 3. Add the Foreign Key constraints
ALTER TABLE hmsmaster.channel_branch_heirarchy
    ADD CONSTRAINT fk_location_created_by FOREIGN KEY (created_by) REFERENCES hms."user"(user_id),
    ADD CONSTRAINT fk_location_modified_by FOREIGN KEY (modified_by) REFERENCES hms."user"(user_id);


alter table hms.sr_approver add column decision_comment varchar(4000);

alter table hms.sr_approver add reporting_mgr int4 references hms."user"(user_id);
ALTER TABLE hms.inbox 
DROP CONSTRAINT IF EXISTS inbox_cntrl_id_fkey;

ALTER TABLE hms.inbox 
DROP COLUMN IF EXISTS cntrl_id;

ALTER TABLE hms.inbox 
ADD COLUMN component_id INT4 
REFERENCES hmsmaster.ui_components(component_id);

UPDATE hms.inbox SET component_id = 135;

ALTER TABLE hms.inbox 
ALTER COLUMN component_id SET NOT NULL;

alter table hms."user" add refreshtoken varchar(500);
alter table hms."user" add refreshtokenexpirytime timestamp;
create unique index uq_usr_org_reftkn on hms."user"(orgid, refreshtoken);

update hmsmaster.keyvalueentries  set entrycategory ='BackgroundJobUserName' where entrydesc = 'backgroundjob'

update hmsmaster.keyvalueentries k set entrydesc = 'backgroundjob' where k.entrycategory ='BackgroundJobUserName'

update hmsmaster.keyvalueentries k set entrydesc = 'backgroundjob#123' where k.entrycategory ='BackgroundJobUserPassword'

ALTER TABLE hms.sr_approver ALTER COLUMN allocatedroleid DROP NOT NULL;

alter table hms.inbox  add column object_ref int4;

update hms.inbox  set object_ref = 108

ALTER TABLE hms.inbox ALTER COLUMN object_ref SET NOT NULL;

alter table hms.inbox add column comments varchar(2000)

ALTER TABLE hms.role_menu_mapping  ALTER COLUMN mapping_id DROP DEFAULT;

ALTER TABLE hms.role_menu_mapping  ALTER COLUMN mapping_id ADD GENERATED BY DEFAULT AS IDENTITY (SEQUENCE NAME hms.rolemapping_seq);

SELECT setval(pg_get_serial_sequence('hms.role_menu_mapping', 'mapping_id'), coalesce(max(mapping_id), 0) + 1, false) 
FROM hms.role_menu_mapping;

alter table hmsmaster.ui_fields_setting drop column approveroneid;
alter table hmsmaster.ui_fields_setting drop column approvertwoid;
alter table hmsmaster.ui_fields_setting drop column approverthreeid;
alter table hmsmaster.ui_fields_setting drop column usedefaultapprover;
