import { BaseApi } from "@/$core/base-api";
import { IC_Printer } from "@/models/ic-printer";
import { PagedList } from "@/models/paged-list";
import { AxiosRequestConfig } from "axios";

class PrinterApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public async getAllPrinterAsync() {
        const res = await this.get('GetAllPrinter');
        return res.data as IC_Printer[];
    }

    public async createPrinterAsync(payload: {
        Name: string,
        SerialNumber: string,
        IPAddress: string,
        PrinterModel?: string,
        Port: number,
    }) {
        const res = await this.post("Create", payload);
        return res.data as IC_Printer;
    }

    public async deleteManyPrinterAsync(printerIndexes: number[]) {
        const params = new URLSearchParams();
        printerIndexes.forEach(p => params.append('printerIndexes', p.toString()));
        await this.delete("DeleteMany", {
            params
        });
    }

    public async updatePrinterAsync(payload: {
        Index: number,
        Name: string,
        SerialNumber: string,
        IPAddress: string,
        PrinterModel?: string,
        Port: number,
    }) {
        const res = await this.put("Update", payload);
        return res.data as IC_Printer;
    }

    public async getPrintersAsync(payload: {
        searchValue?: string,
        page: number,
        pageSize: number,
    }) {
        const res = await this.get('GetPrinters', {
            params: payload
        });
        return res.data as PagedList<IC_Printer>;
    }

    public async printTestPageAsync(payload: {
        printerName: string,
    }) {
        await this.post("TestPrinter", null, {
            params: payload
        });
    }
}

export const printerApi = new  PrinterApi('Printer');