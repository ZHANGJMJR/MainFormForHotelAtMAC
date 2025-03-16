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

 Date: 16/03/2025 19:50:44
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for crontab
-- ----------------------------
DROP TABLE IF EXISTS `crontab`;
CREATE TABLE `crontab` (
  `id` int NOT NULL AUTO_INCREMENT,
  `kind` int DEFAULT NULL COMMENT '1代表给审计的',
  `cronexpress` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_bin;

-- ----------------------------
-- Table structure for fuzhanggui
-- ----------------------------
DROP TABLE IF EXISTS `fuzhanggui`;
CREATE TABLE `fuzhanggui` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `storename` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '门店名称',
  `storeid` bigint DEFAULT NULL COMMENT '门店ID',
  `itemname` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '商品名称',
  `paymethod` varchar(128) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '支付类型',
  `subitemname` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '子规格商品名',
  `property` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '属性',
  `barcode` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '商品条形码',
  `itemgroup` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '商品一级分组',
  `finitemgroup` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '财务分组',
  `ordernum` bigint DEFAULT NULL COMMENT '订单号',
  `orderdate` date DEFAULT NULL COMMENT '下单日期',
  `ordertime` time DEFAULT NULL COMMENT '下单时间',
  `paydate` date DEFAULT NULL COMMENT '结账日期',
  `paytime` time DEFAULT NULL COMMENT '结账时间',
  `price` decimal(10,2) DEFAULT NULL COMMENT '单价',
  `qty` decimal(20,6) DEFAULT NULL COMMENT '数量',
  `unit` varchar(64) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '单位',
  `orgprice` decimal(10,2) DEFAULT NULL COMMENT '原单价格',
  `disprice` decimal(10,2) DEFAULT NULL COMMENT '优惠金额',
  `actualmount` decimal(10,2) DEFAULT NULL COMMENT '实收金额（含抹零金额）',
  `refund` decimal(10,2) DEFAULT NULL COMMENT '退款金额',
  `discount` decimal(10,6) DEFAULT NULL COMMENT '折扣率',
  `channel` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '销售渠道',
  `pickcode` varchar(64) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '取餐码',
  `tableref` varchar(64) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '桌台',
  `realinventory` decimal(10,2) DEFAULT NULL COMMENT '实时库存',
  `member` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_bin DEFAULT NULL COMMENT '是否会员消费',
  `membername` varchar(64) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '会员名称',
  `memberlevel` varchar(32) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '会员等级',
  `membercardno` varchar(128) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '会员实体卡号',
  `orderstaff` varchar(64) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '下单人',
  `opentable` varchar(64) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '开台人',
  `itemmemo` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '商品备注',
  `ordermemo` varchar(255) COLLATE utf8mb3_bin DEFAULT NULL COMMENT '订单备注',
  `insert_dt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1746 DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_bin;

-- ----------------------------
-- Table structure for fzgfiles
-- ----------------------------
DROP TABLE IF EXISTS `fzgfiles`;
CREATE TABLE `fzgfiles` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `filename` varchar(255) COLLATE utf8mb3_bin NOT NULL,
  `filedata` longblob NOT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_bin;

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
) ENGINE=InnoDB AUTO_INCREMENT=7606 DEFAULT CHARSET=utf8mb3 ROW_FORMAT=DYNAMIC;

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
) ENGINE=InnoDB AUTO_INCREMENT=24329 DEFAULT CHARSET=utf8mb3 ROW_FORMAT=DYNAMIC;

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
) ENGINE=InnoDB AUTO_INCREMENT=6338 DEFAULT CHARSET=utf8mb3 ROW_FORMAT=DYNAMIC;

SET FOREIGN_KEY_CHECKS = 1;
INSERT INTO `hotel`.`crontab` (`id`, `kind`, `cronexpress`) VALUES (1, 1, '0 0 1 * * ? *');
