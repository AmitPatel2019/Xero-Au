export interface PopupResultUserClosed {
    userClosed: true;
}
export interface PopupResultReceivedUrl {
    userClosed: false;
    receivedUrl: string;
}
export declare type PopupResult = PopupResultUserClosed | PopupResultReceivedUrl;
