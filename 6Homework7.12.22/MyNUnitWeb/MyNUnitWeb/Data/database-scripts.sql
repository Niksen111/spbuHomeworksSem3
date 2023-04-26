--/*
CREATE TABLE assembly (
    name TEXT NOT NULL,
    tests_count INT NOT NULL,
    passed INT NOT NULL,
    failed INT NOT NULL,
    ignored INT NOT NULL,
    datetime TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
--*/

--/*
CREATE TABLE test (
    id_assembly INT,
    name TEXT NOT NULL,
    is_passed INT NOT NULL,
    running_time INT NOT NULL,
    reason_for_ignoring TEXT,
    comment TEXT,
    FOREIGN KEY(id_assembly) REFERENCES assembly(ROWID)
);
--*/

INSERT INTO assembly (name, tests_count, passed, failed, ignored) 
values ('assembly_name1', 3, 1, 1, 1);

INSERT INTO test (id_assembly, name, is_passed, running_time, reason_for_ignoring, comment)
values (1, 'test_name3', FALSE, 0, 'Ignored.', '');

SELECT * FROM assembly;
SELECT * FROM assembly WHERE ROWID = 1;
SELECT * FROM test;

DROP TABLE assembly;
DROP TABLE test;
