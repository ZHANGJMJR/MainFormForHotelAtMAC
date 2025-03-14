/*
 Navicat Premium Data Transfer

 Source Server         : 1111
 Source Server Type    : MySQL
 Source Server Version : 90200
 Source Host           : 127.0.0.1:3306
 Source Schema         : hotel

 Target Server Type    : MySQL
 Target Server Version : 90200
 File Encoding         : 65001

 Date: 14/03/2025 21:42:00
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for guestcheck
-- ----------------------------
DROP TABLE IF EXISTS `guestcheck`;
CREATE TABLE `guestcheck` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `guestcheckid` bigint NOT NULL,
  `busdate` datetime DEFAULT NULL,
  `locationid` bigint DEFAULT '2041',
  `revenuecenterid` bigint DEFAULT '12950',
  `checkNum` bigint DEFAULT NULL,
  `openDateTime` datetime DEFAULT NULL,
  `checkTotal` decimal(60,6) DEFAULT '0.000000',
  `numItems` bigint DEFAULT '0',
  `firstName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `lastName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `is_download` int DEFAULT '0' COMMENT '0是未下载 ，其余为已下载次数',
  `downoad_datetime` datetime DEFAULT NULL COMMENT '上次下载的时间',
  `getdatadate` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT '记录日期2024-07-01',
  `insert_dt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1593 DEFAULT CHARSET=utf8mb3 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for guestcheckdetails
-- ----------------------------
DROP TABLE IF EXISTS `guestcheckdetails`;
CREATE TABLE `guestcheckdetails` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transTime` datetime DEFAULT NULL,
  `serviceRoundNum` bigint DEFAULT NULL,
  `lineNum` bigint DEFAULT NULL,
  `guestCheckLineItemID` bigint DEFAULT NULL,
  `detailType` int DEFAULT NULL,
  `itemName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `itemName2` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT 'reference infor name',
  `itemchname` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `rvcName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `firstName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `lastName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `reasonVoidText` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `returnText` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `recordID` bigint DEFAULT NULL,
  `salesTotal` decimal(20,6) DEFAULT NULL,
  `salesCount` int DEFAULT NULL,
  `salesCountDivisor` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `locationID` bigint DEFAULT '2041',
  `doNotShow` int DEFAULT NULL,
  `guestCheckID` bigint DEFAULT NULL,
  `organizationID` bigint DEFAULT '10260',
  `checkNum` bigint DEFAULT NULL,
  `insert_dt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `openDatetime` datetime DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2868 DEFAULT CHARSET=utf8mb3 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for guestcheckdetailssumrow
-- ----------------------------
DROP TABLE IF EXISTS `guestcheckdetailssumrow`;
CREATE TABLE `guestcheckdetailssumrow` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `organizationID` bigint DEFAULT NULL,
  `checkNum` bigint DEFAULT NULL,
  `tableRef` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `openDatetime` datetime DEFAULT NULL,
  `duration` decimal(19,6) DEFAULT NULL,
  `numGuests` int DEFAULT NULL,
  `checkRef` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `locName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `rvcName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `otName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `firstName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `lastName` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `guestCheckID` bigint DEFAULT NULL,
  `locationID` int DEFAULT '2041',
  `insert_dt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=421 DEFAULT CHARSET=utf8mb3 ROW_FORMAT=DYNAMIC;

SET FOREIGN_KEY_CHECKS = 1;
