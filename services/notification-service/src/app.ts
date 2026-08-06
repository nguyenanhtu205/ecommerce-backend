import express, {Express} from 'express';
import cors from 'cors';
import helmet from 'helmet';
import {routes} from './routes';
import {currentUserMiddleware} from "./middlewares/currentUser.middleware";

export function createApp(): Express {
  const app = express();

  app.use(helmet());
  app.use(cors());
  app.use(express.json());
  app.use(currentUserMiddleware);

  app.use(routes);

  return app;
}
