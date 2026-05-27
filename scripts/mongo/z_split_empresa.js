db.DocumentosMetadata.updateMany({ id_documento_sql: { $in: [1, 2, 3, 4, 5, 6, 7, 8] } }, { $set: { id_empresa: 2 } });
db.DocumentosMetadata.updateMany({ id_documento_sql: { $in: [9, 10, 11, 12, 13, 14, 15, 16] } }, { $set: { id_empresa: 3 } });
