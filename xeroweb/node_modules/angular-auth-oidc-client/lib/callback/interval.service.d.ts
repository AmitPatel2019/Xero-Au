import { NgZone } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import * as i0 from "@angular/core";
export declare class IntervalService {
    private zone;
    runTokenValidationRunning: Subscription;
    constructor(zone: NgZone);
    stopPeriodicTokenCheck(): void;
    startPeriodicTokenCheck(repeatAfterSeconds: number): Observable<unknown>;
    static ɵfac: i0.ɵɵFactoryDeclaration<IntervalService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<IntervalService>;
}
