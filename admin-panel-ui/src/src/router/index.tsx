import { Navigate, createBrowserRouter } from "react-router-dom";
import Layout from "../layout";
import Streams from "../pages/streams";
import { getEnvConfig } from "../common/env";

export const router = createBrowserRouter([
  {
    path: getEnvConfig().BASE_PATH,
    element: <Layout />,
    children: [
      {
        index: true,
        element: <Navigate to="streams" />,
      },
      {
        path: "streams",
        element: <Streams />,
      },
    ],
  },
  {
    path: "*",
    element: <Navigate to={getEnvConfig().BASE_PATH} />,
  },
]);
