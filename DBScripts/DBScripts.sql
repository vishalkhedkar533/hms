CREATE SCHEMA IF NOT EXISTS hms;
CREATE TABLE hms.permissions (
    permission_id   VARCHAR(30) NOT NULL,
    permission_name VARCHAR(100) NOT NULL,
    description     VARCHAR(255),
    module_name     VARCHAR(50),
    is_active       BOOLEAN NOT NULL,
    created_by      VARCHAR(100) NOT NULL,
    created_date    TIMESTAMP NOT NULL,
    modified_by     VARCHAR(100),
    modified_date   TIMESTAMP,
    rowversion      INTEGER,
    PRIMARY KEY (permission_id)
);

DROP TABLE hms.permissions;

CREATE TABLE hms.permissions (
    permission_id   INTEGER NOT NULL PRIMARY KEY, -- numeric, not auto-generated
    permission_name VARCHAR(100) NOT NULL,
    description     VARCHAR(255),
    module_name     VARCHAR(50),
    is_active       BOOLEAN NOT NULL,
    created_by      VARCHAR(100) NOT NULL,
    created_date    TIMESTAMP NOT NULL,
    modified_by     VARCHAR(100),
    modified_date   TIMESTAMP,
    rowversion      INTEGER
);


CREATE TABLE hms.roles (
    role_id         INTEGER NOT NULL PRIMARY KEY,
    role_name       VARCHAR(100) NOT NULL,
    description     VARCHAR(255),
    is_system_role  BOOLEAN NOT NULL,
    is_active       BOOLEAN NOT NULL,
    created_by      VARCHAR(100) NOT NULL,
    created_date    TIMESTAMP NOT NULL,
    modified_by     VARCHAR(100),
    modified_date   TIMESTAMP,
    rowversion      INTEGER
);


CREATE TABLE hms."user" (
    user_id         SERIAL PRIMARY KEY,
    username        VARCHAR(100) NOT NULL,
    email_id        VARCHAR(150) NOT NULL,
    mobile_number   VARCHAR(20),
    password        VARCHAR(255) NOT NULL,
    is_active       BOOLEAN NOT NULL,
    is_locked       BOOLEAN NOT NULL,
    last_login_date TIMESTAMP,
    created_by      VARCHAR(100) NOT NULL,
    created_date    TIMESTAMP NOT NULL,
    modified_by     VARCHAR(100),
    modified_date   TIMESTAMP,
    rowversion      INTEGER
);

CREATE TABLE hms.user_role_mapping (
    mapping_id      BIGINT NOT NULL PRIMARY KEY,
    user_id         INTEGER NOT NULL,
    role_id         INTEGER NOT NULL,
    is_primary      BOOLEAN NOT NULL,
    effective_from  DATE NOT NULL,
    effective_to    DATE,
    is_active       BOOLEAN NOT NULL,
    created_by      VARCHAR(100) NOT NULL,
    created_date    TIMESTAMP NOT NULL,
    modified_by     VARCHAR(100),
    modified_date   TIMESTAMP,
    rowversion      INTEGER,

    CONSTRAINT fk_user
      FOREIGN KEY (user_id) REFERENCES hms.user(user_id),

    CONSTRAINT fk_role
      FOREIGN KEY (role_id) REFERENCES hms.roles(role_id)
);
