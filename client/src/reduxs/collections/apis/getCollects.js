
import { collectService } from "../actions/collect.actions.js";
import { requestGet } from "../../../_axios/axiosClient";
import { controller } from "../../../controller/apis/controller.js";

export const getCollects = () =>{
    return async dispatch => {
        if(localStorage.getItem("persist:__col")) return;
        await dispatch(collectService.request());

        let url = controller.GET_COLLECTES;
        const response = await requestGet(url);
        
        if (response.code > 204) {
            await dispatch(collectService.failture("Error"));
        }
        else {
            await dispatch(collectService.success(response.data));
        }
    }
}