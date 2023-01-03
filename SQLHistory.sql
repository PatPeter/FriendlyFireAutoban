

-- 2022-06-14
ALTER TABLE `scpsl_unigamia`.`scpsl_user_id_bans` 
ADD COLUMN `date_unbanned` DATETIME NULL DEFAULT NULL AFTER `unbanned`,
ADD COLUMN `date_modified` DATETIME NULL DEFAULT NULL AFTER `date_display`,
CHANGE COLUMN `display_end_date` `date_display` DATETIME NULL DEFAULT NULL ,
CHANGE COLUMN `display_start_date` `date_created` DATETIME NULL DEFAULT NULL ;

ALTER TABLE `scpsl_unigamia`.`scpsl_ip_bans` 
ADD COLUMN `date_unbanned` DATETIME NULL DEFAULT NULL AFTER `unbanned`,
ADD COLUMN `date_display` DATETIME NULL DEFAULT NULL AFTER `date_unbanned`,
ADD COLUMN `date_modified` DATETIME NULL DEFAULT NULL AFTER `date_display`,
ADD COLUMN `date_created` DATETIME NULL DEFAULT NULL AFTER `date_modified`;



DROP TRIGGER IF EXISTS `scpsl_unigamia`.`scpsl_user_id_bans_BEFORE_INSERT`;

DELIMITER $$
USE `scpsl_unigamia`$$
CREATE DEFINER=`scpsl`@`localhost` TRIGGER `scpsl_unigamia`.`scpsl_user_id_bans_BEFORE_INSERT` BEFORE INSERT ON `scpsl_user_id_bans` FOR EACH ROW
BEGIN
	IF NEW.end_date < 636503616170000000 -- 2018-01-01 00:00:17
		OR NEW.start_date < 636503616170000000
	THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ban created with start date or end date before 2018.';
	END IF;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS `scpsl_unigamia`.`scpsl_ip_bans_BEFORE_INSERT`;

DELIMITER $$
USE `scpsl_unigamia`$$
CREATE DEFINER=`scpsl`@`localhost` TRIGGER `scpsl_ip_bans_BEFORE_INSERT` BEFORE INSERT ON `scpsl_ip_bans` FOR EACH ROW BEGIN
	IF NEW.end_date < 636503616170000000 -- 2018-01-01 00:00:17
		OR NEW.start_date < 636503616170000000
	THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ban created with start date or end date before 2018.';
	END IF;
END$$
DELIMITER ;

SET SQL_SAFE_UPDATES = 0;

-- DELETE FROM scpsl_user_id_bans WHERE id = 13654;
-- DELETE FROM scpsl_user_id_bans WHERE id = 5988;
-- DELETE FROM scpsl_user_id_bans WHERE id = 6173;
-- SELECT * FROM scpsl_user_id_bans WHERE user_id = '76561198877453127@steam' AND end_date = 652573631124072530 ; -- 1235

-- DELETE FROM scpsl_user_id_bans WHERE id = 13655;
-- SELECT * FROM scpsl_user_id_bans WHERE user_id = '76561198447338613@steam' AND end_date = 637150098340000000; -- 6181

-- DELETE FROM scpsl_user_id_bans WHERE id = 6407;
-- DELETE FROM scpsl_user_id_bans WHERE id = 13656;
-- SELECT * FROM scpsl_user_id_bans WHERE user_id = '76561198868518061@steam' AND end_date = 652743380629150470; -- 3845

-- DELETE FROM scpsl_user_id_bans WHERE id = 13657;
-- DELETE FROM scpsl_user_id_bans WHERE id = 8826;
-- SELECT * FROM scpsl_user_id_bans WHERE user_id = '76561198364814880@steam' AND end_date = 637350447508893270; -- 8750

-- SELECT * FROM scpsl_user_id_bans WHERE start_date < 636503616170000000;

UPDATE scpsl_user_id_bans SET
    date_created = FROM_DOTNETTICKS(start_date)
WHERE
	date_created IS NULL;

UPDATE scpsl_user_id_bans SET
    date_modified = date_created
WHERE
	date_modified IS NULL;

UPDATE scpsl_user_id_bans SET
    date_display = FROM_DOTNETTICKS(end_date)
WHERE
	date_display IS NULL;

UPDATE scpsl_user_id_bans SET
    date_modified = FROM_DOTNETTICKS(end_date)
WHERE
	`active` = 0 AND `unbanned` = 0;

ALTER TABLE `scpsl_unigamia`.`scpsl_user_id_bans` 
CHANGE COLUMN `date_display` `date_display` DATETIME NOT NULL ,
CHANGE COLUMN `date_modified` `date_modified` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP ,
CHANGE COLUMN `date_created` `date_created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ;

-- DELETE FROM scpsl_ip_bans WHERE id = 10812;
-- DELETE FROM scpsl_ip_bans WHERE id = 5134;
-- SELECT * FROM scpsl_ip_bans WHERE user_id = '73.74.44.103' AND end_date = 637134321903887160 ;

-- DELETE FROM scpsl_ip_bans WHERE id = 5822;
-- DELETE FROM scpsl_ip_bans WHERE id = 10813;
-- SELECT * FROM scpsl_ip_bans WHERE user_id = '50.53.130.144' AND end_date = 652446129555760100 ; -- 209

-- SELECT * FROM scpsl_ip_bans WHERE start_date < 636503616170000000;

UPDATE scpsl_ip_bans SET
    date_created = FROM_DOTNETTICKS(start_date)
WHERE
	date_created IS NULL;

UPDATE scpsl_ip_bans SET
    date_modified = date_created
WHERE
	date_modified IS NULL;

UPDATE scpsl_ip_bans SET
    date_display = FROM_DOTNETTICKS(end_date)
WHERE
	date_display IS NULL;

UPDATE scpsl_ip_bans SET
    date_modified = FROM_DOTNETTICKS(end_date)
WHERE
	`active` = 0 AND `unbanned` = 0;

ALTER TABLE `scpsl_unigamia`.`scpsl_ip_bans` 
CHANGE COLUMN `date_display` `date_display` DATETIME NOT NULL ,
CHANGE COLUMN `date_modified` `date_modified` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP ,
CHANGE COLUMN `date_created` `date_created` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ;

DROP TRIGGER IF EXISTS `scpsl_unigamia`.`scpsl_user_id_bans_BEFORE_UPDATE`;

DELIMITER $$
USE `scpsl_unigamia`$$
CREATE DEFINER = `scpsl`@`localhost` TRIGGER `scpsl_unigamia`.`scpsl_user_id_bans_BEFORE_UPDATE` BEFORE UPDATE ON `scpsl_user_id_bans` FOR EACH ROW
BEGIN
    -- If a readonly column is being modified
    IF OLD.id != NEW.id
        OR OLD.user_id != NEW.user_id
		OR OLD.end_date != NEW.end_date
		OR OLD.start_date != NEW.start_date
	THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unique ban columns are read-only.';
	END IF;
END$$
DELIMITER ;

DROP TRIGGER IF EXISTS `scpsl_unigamia`.`scpsl_ip_bans_BEFORE_UPDATE`;

DELIMITER $$
USE `scpsl_unigamia`$$
CREATE DEFINER = `scpsl`@`localhost` TRIGGER `scpsl_unigamia`.`scpsl_ip_bans_BEFORE_UPDATE` BEFORE UPDATE ON `scpsl_ip_bans` FOR EACH ROW
BEGIN
	-- If a readonly column is being modified
    IF OLD.id != NEW.id
        OR OLD.user_id != NEW.user_id
		OR OLD.end_date != NEW.end_date
		OR OLD.start_date != NEW.start_date
	THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unique ban columns are read-only.';
	END IF;
END$$
DELIMITER ;

