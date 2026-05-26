import mongoose, { Document } from 'mongoose';
import pino from 'pino';
export declare const logger: pino.Logger<never, boolean>;
export declare const app: import("express-serve-static-core").Express;
interface IMetadato extends Document {
    id_documento_sql: number;
    id_empresa: number;
    codigo_interno: string;
    titulo: string;
    tags: string[];
    version?: number;
    contenido_extraido?: string;
    id_usuario_creacion: number;
    estatus: boolean;
    fecha_indexacion: Date;
    fecha_modificacion?: Date | null;
    id_usuario_modificacion?: number | null;
    fecha_eliminacion?: Date | null;
    id_usuario_eliminacion?: number | null;
}
export declare const Metadato: mongoose.Model<IMetadato, {}, {}, {}, mongoose.Document<unknown, {}, IMetadato, {}, mongoose.DefaultSchemaOptions> & IMetadato & Required<{
    _id: mongoose.Types.ObjectId;
}> & {
    __v: number;
} & {
    id: string;
}, any, IMetadato>;
export declare function escapeRegex(text: string): string;
export {};
//# sourceMappingURL=index.d.ts.map