import { LoggerService } from '../../logging/logger.service';
import * as i0 from "@angular/core";
export declare class RandomService {
    private readonly doc;
    private loggerService;
    constructor(doc: any, loggerService: LoggerService);
    createRandom(requiredLength: number, configId: string): string;
    private toHex;
    private randomString;
    private getCrypto;
    static ɵfac: i0.ɵɵFactoryDeclaration<RandomService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<RandomService>;
}
