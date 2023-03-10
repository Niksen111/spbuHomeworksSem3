/*
CREATE TABLE assembly (
    id_assembly INT PRIMARY KEY DEFAULT ROWID, 
    tests_count INT NOT NULL,
    passed INT NOT NULL,
    failed INT NOT NULL,
    ignored INT NOT NULL,
    datetime TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
*/

--INSERT INTO assembly (tests_number, passed, failed, ignored) 
--values (10, 5, 4, 0);

--SELECT * FROM assembly;

--DROP TABLE assembly;

/*
CREATE TABLE test (
    id_test INT PRIMARY KEY DEFAULT ROWID,
    id_assembly INT,
    name TEXT NOT NULL,
    is_passed INT NOT NULL,
    running_time INT NOT NULL,
    reason_for_ignoring TEXT,
    comment TEXT,
    FOREIGN KEY(id_assembly) REFERENCES assembly(id_assembly)
);
*/

--SELECT * FROM test;

--DROP TABLE test;