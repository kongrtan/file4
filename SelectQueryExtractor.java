package com.hyunsoft.stock8888.common.util;

import com.p6spy.engine.spy.appender.MessageFormattingStrategy;

public class SelectQueryExtractor implements MessageFormattingStrategy {

	public static final ThreadLocal<String> lastSelectSql = new ThreadLocal<>();

	@Override
	public String formatMessage(int connectionId, String now, long elapsed, String category, String prepared, String sql, String url) {
		if (sql == null || sql.trim().isEmpty()) return "";

		System.out.println("SelectQueryExtractor=" + sql);

		if (sql.contains("formula_tp")) {
			lastSelectSql.set(sql); // 원본 SQL 저장
		}

		return String.format("[P6Spy] %d ms | %s", elapsed, sql);
	}
}