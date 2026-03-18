-- =============================================================================
-- UpdateStatistics
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS UpdateStatistics$$
CREATE PROCEDURE UpdateStatistics()
BEGIN
    DECLARE v_table VARCHAR(128);
    DECLARE v_sql   LONGTEXT;
    DECLARE done    INT DEFAULT 0;
    DECLARE cur CURSOR FOR
        SELECT TABLE_NAME FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

    OPEN cur;
    read_loop: LOOP
        FETCH cur INTO v_table;
        IF done THEN LEAVE read_loop; END IF;
        SET v_sql = CONCAT('ANALYZE TABLE `', v_table, '`');
        SET @_stmt = v_sql;
        PREPARE _s FROM @_stmt;
        EXECUTE _s;
        DEALLOCATE PREPARE _s;
    END LOOP;
    CLOSE cur;
END$$

DELIMITER ;
