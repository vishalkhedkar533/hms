ALTER TABLE hms."user"
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


alter table hms."user" ADD COLUMN OrgId INTEGER;

alter table hms."user" 
add CONSTRAINT fk_User_OrgId
FOREIGN KEY (OrgId) REFERENCES hms.Organisation(OrgId)