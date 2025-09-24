export default class ResponseModel {
    public meta: {
        error_code: number;
        error_message: string;
    } | undefined;

    public data: any;

    public metadata: any;
}