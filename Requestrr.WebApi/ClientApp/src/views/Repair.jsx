/*!

=========================================================
* Requestrr-repair fork — /repair admin panel
=========================================================

*/

import { useEffect, useState } from "react";
import { useDispatch } from 'react-redux';
import { Alert } from "reactstrap";
import { getRepairSettings, saveRepairSettings } from "../store/actions/RepairActions";
import MultiDropdown from "../components/Inputs/MultiDropdown";

// reactstrap components
import {
  Card,
  CardHeader,
  CardBody,
  FormGroup,
  Form,
  Input,
  Container,
  Row,
  Col
} from "reactstrap";
// core components
import UserHeader from "../components/Headers/UserHeader.jsx";


function Repair() {

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [saveAttempted, setSaveAttempted] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveError, setSaveError] = useState("");

  const [enabled, setEnabled] = useState(false);
  const [deleteFileBeforeReSearch, setDeleteFileBeforeReSearch] = useState(true);
  const [monitoredRoles, setMonitoredRoles] = useState([]);

  const dispatch = useDispatch();


  useEffect(() => {
    dispatch(getRepairSettings())
      .then(data => {
        setIsLoading(false);
        const payload = data.payload || {};
        setEnabled(!!payload.enabled);
        setDeleteFileBeforeReSearch(payload.deleteFileBeforeReSearch !== false);
        setMonitoredRoles(Array.isArray(payload.monitoredRoles) ? payload.monitoredRoles : []);
      });
  }, []);


  useEffect(() => {
    if (!isSaving)
      return;

    dispatch(saveRepairSettings({
      'enabled': enabled,
      'deleteFileBeforeReSearch': deleteFileBeforeReSearch,
      'monitoredRoles': monitoredRoles
    }))
      .then(data => {
        setIsSaving(false);

        if (data.ok) {
          setSaveAttempted(true);
          setSaveError("");
          setSaveSuccess(true);
        } else {
          let error = "An unknown error occurred while saving.";

          if (typeof (data.error) === "string")
            error = data.error;

          setSaveAttempted(true);
          setSaveError(error);
          setSaveSuccess(false);
        }
      });
  }, [isSaving]);


  const onSaving = e => {
    e.preventDefault();

    if (!isSaving) {
      setIsSaving(true);
    }
  };


  return (
    <>
      <UserHeader title="Repair" description="Configure the /repair Discord slash command for re-grabbing broken media." />
      <Container className="mt--7" fluid>
        <Row>
          <Col className="order-xl-1" xl="12">
            <Card className="bg-secondary shadow">
              <CardHeader className="bg-white border-0">
                <Row className="align-items-center">
                  <Col xs="12">
                    <h3 className="mb-0">Repair (re-download broken files)</h3>
                  </Col>
                </Row>
              </CardHeader>
              <CardBody className={isLoading ? "fade" : "fade show"}>
                <Form className="complex">
                  <h6 className="heading-small text-muted mb-4">
                    /repair Discord command
                  </h6>
                  <div className="pl-lg-4">
                    <Row>
                      <Col md="12">
                        <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                          <Input
                            className="custom-control-input"
                            id="repairEnabled"
                            type="checkbox"
                            onChange={e => setEnabled(!enabled)}
                            checked={enabled}
                          />
                          <label
                            className="custom-control-label"
                            htmlFor="repairEnabled"
                          >
                            <span className="text-muted">Enable /repair Discord command</span>
                          </label>
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col md="12">
                        <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                          <Input
                            className="custom-control-input"
                            id="repairDeleteFileBeforeReSearch"
                            type="checkbox"
                            onChange={e => setDeleteFileBeforeReSearch(!deleteFileBeforeReSearch)}
                            checked={deleteFileBeforeReSearch}
                          />
                          <label
                            className="custom-control-label"
                            htmlFor="repairDeleteFileBeforeReSearch"
                          >
                            <span className="text-muted">Delete file before re-search</span>
                          </label>
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col lg="12">
                        <FormGroup>
                          <MultiDropdown
                            name="Roles allowed to use /repair"
                            create={true}
                            searchable={true}
                            placeholder="Enter role ids here. Leave blank for all roles."
                            labelField="name"
                            valueField="id"
                            dropdownHandle={false}
                            selectedItems={monitoredRoles.map(x => { return { name: x, id: x } })}
                            items={monitoredRoles.map(x => { return { name: x, id: x } })}
                            onChange={newRoles => setMonitoredRoles(newRoles.filter(x => /\S/.test(x.id)).map(x => x.id.trim()))} />
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col>
                        <FormGroup>
                          <Alert color="warning">
                            <strong>Warning:</strong> You must restart the bot for any changes made on this page to take effect.
                          </Alert>
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col>
                        <FormGroup>
                          {
                            saveAttempted && !isSaving ?
                              !saveSuccess ? (
                                <Alert className="text-center" color="danger">
                                  <strong>{saveError}</strong>
                                </Alert>)
                                : <Alert className="text-center" color="success">
                                  <strong>Settings updated successfully.</strong>
                                </Alert>
                              : null
                          }
                        </FormGroup>
                        <FormGroup className="text-right">
                          <button className="btn btn-icon btn-3 btn-primary" onClick={onSaving} disabled={isSaving} type="button">
                            <span className="btn-inner--icon"><i className="fas fa-save"></i></span>
                            <span className="btn-inner--text">Save Changes</span>
                          </button>
                        </FormGroup>
                      </Col>
                    </Row>
                  </div>
                </Form>
              </CardBody>
            </Card>
          </Col>
        </Row>
      </Container>
    </>
  );
}

export default Repair;
