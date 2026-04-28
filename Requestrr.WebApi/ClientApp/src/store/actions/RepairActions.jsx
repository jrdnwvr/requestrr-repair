export const GET_REPAIR_SETTINGS = "repair:get_settings";

export function setRepairSettings(settings) {
    return {
        type: GET_REPAIR_SETTINGS,
        payload: settings
    };
};

export function getRepairSettings() {
    return (dispatch, getState) => {

        return fetch("../api/configuration/repair", {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        })
            .then(data => data.json())
            .then(data => {
                return dispatch(setRepairSettings(data));
            });
    };
};

export function saveRepairSettings(saveModel) {
    return (dispatch, getState) => {
        const state = getState();

        return fetch("../api/configuration/repair", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${state.user.token}`
            },
            body: JSON.stringify({
                'Enabled': !!saveModel.enabled,
                'DeleteFileBeforeReSearch': !!saveModel.deleteFileBeforeReSearch,
                'MonitoredRoles': saveModel.monitoredRoles || []
            })
        })
            .then(data => data.json())
            .then(data => {
                if (data.ok) {
                    dispatch(setRepairSettings({
                        enabled: saveModel.enabled,
                        deleteFileBeforeReSearch: saveModel.deleteFileBeforeReSearch,
                        monitoredRoles: saveModel.monitoredRoles
                    }));
                    return { ok: true };
                }

                return { ok: false, error: data };
            });
    }
};
